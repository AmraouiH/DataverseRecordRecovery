using System;
using System.Collections.Generic;
using System.Linq;
using Dataverse_Record_Recovery.Localization;
using Dataverse_Record_Recovery.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Dataverse_Record_Recovery.Services
{
    /// <summary>
    /// Builds the list of records to restore (selected records, deleted related records, missing parents)
    /// and recreates them with their original GUID
    /// </summary>
    public class RecoveryService
    {
        private const int MaxChildDepth = 3;
        private const int MaxParentDepth = 5;
        private const int MaxChildCandidates = 5000;
        private const int ExistenceBatchSize = 300;

        private readonly IOrganizationService service;
        private readonly MetadataCache metadata;
        private readonly AuditService audit;

        public RecoveryService(IOrganizationService service, MetadataCache metadata, AuditService audit)
        {
            this.service = service;
            this.metadata = metadata;
            this.audit = audit;
        }

        #region Plan

        public RecoveryPlan BuildPlan(IEnumerable<DeletedRecord> selected, bool includeRelated, int relatedWindowSeconds, Action<string> progress)
        {
            var plan = new RecoveryPlan();

            // A record deleted several times is restored from its most recent deletion
            foreach (var record in selected.Where(r => r.OldValue != null).OrderByDescending(r => r.DeletedOn))
            {
                if (plan.ById.ContainsKey(record.RecordId))
                {
                    continue;
                }
                var item = new RecoveryItem { Record = record, Role = RecoveryRole.Selected };
                plan.Roots.Add(item);
                plan.Add(item);
            }

            if (includeRelated)
            {
                var wave = plan.Items.ToList();
                for (var depth = 0; depth < MaxChildDepth && wave.Count > 0; depth++)
                {
                    progress?.Invoke(Strings.SearchingRelated(depth + 1));
                    wave = FindChildren(plan, wave, TimeSpan.FromSeconds(relatedWindowSeconds));
                }
            }

            var toCheck = plan.Items.ToList();
            for (var depth = 0; depth < MaxParentDepth && toCheck.Count > 0; depth++)
            {
                progress?.Invoke(Strings.CheckingReferences(depth + 1));
                toCheck = FindMissingParents(plan, toCheck, progress);
            }

            progress?.Invoke(Strings.CheckingExisting);
            foreach (var group in plan.Items.GroupBy(i => i.Record.EntityLogicalName))
            {
                var ids = group.Select(i => i.Record.RecordId).ToList();
                var existing = FindExisting(group.Key, ids);
                foreach (var item in group)
                {
                    item.AlreadyExists = existing != null && existing.Contains(item.Record.RecordId);
                    plan.Existence[item.Record.RecordId] = item.AlreadyExists;
                }
            }
            RemoveExisting(plan);

            if (includeRelated)
            {
                progress?.Invoke(Strings.SearchingUnlinked);
                FindUnlinkedRecords(plan, TimeSpan.FromSeconds(relatedWindowSeconds));
            }

            progress?.Invoke(Strings.SearchingAssociations);
            FindAssociations(plan);

            progress?.Invoke(Strings.PreparingPreview);
            foreach (var item in plan.Items)
            {
                BuildPreview(plan, item);
            }

            plan.OrderedItems = Sort(plan);
            return plan;
        }

        /// <summary>
        /// Only what is missing is proposed: records that exist again are left out, together with the missing
        /// parents that were searched only because of them. Their deleted related records stay in the plan.
        /// </summary>
        private static void RemoveExisting(RecoveryPlan plan)
        {
            var removed = new HashSet<RecoveryItem>(plan.Items.Where(i => i.AlreadyExists));
            plan.ExistingCount = removed.Count;

            bool changed;
            do
            {
                changed = false;
                foreach (var item in plan.Items.Where(i => i.Role == RecoveryRole.MissingParent && !removed.Contains(i) && removed.Contains(i.Parent)))
                {
                    removed.Add(item);
                    changed = true;
                }
            }
            while (changed);

            foreach (var item in removed)
            {
                plan.Remove(item);
            }
        }

        /// <summary>
        /// Rebuilds from the audit history the N:N associations each record had before its deletion,
        /// and checks which related records still exist
        /// </summary>
        private void FindAssociations(RecoveryPlan plan)
        {
            foreach (var item in plan.Items)
            {
                var relationships = metadata.GetFull(item.Record.EntityLogicalName).ManyToManyRelationships;
                if (relationships == null || relationships.Length == 0)
                {
                    continue;
                }
                var names = new HashSet<string>(relationships.Select(r => r.SchemaName), StringComparer.OrdinalIgnoreCase);

                Dictionary<string, List<EntityReference>> associations;
                try
                {
                    associations = audit.LoadAssociations(item.Record.RecordId, item.Record.DeletedOn);
                }
                catch (Exception)
                {
                    // Audit not readable for associations: the record itself can still be restored
                    continue;
                }

                foreach (var pair in associations.Where(p => names.Contains(p.Key)))
                {
                    foreach (var target in pair.Value)
                    {
                        item.Associations.Add(new AssociationLink { Relationship = pair.Key, Target = target });
                    }
                }
            }

            var targets = plan.Items.SelectMany(i => i.Associations).Where(a => !string.IsNullOrEmpty(a.Target.LogicalName)).ToList();
            foreach (var group in targets.Where(a => !plan.ById.ContainsKey(a.Target.Id) && !plan.Existence.ContainsKey(a.Target.Id)).GroupBy(a => a.Target.LogicalName))
            {
                var ids = group.Select(a => a.Target.Id).Distinct().ToList();
                var existing = RetrieveCurrent(group.Key, ids, new string[0]);
                foreach (var id in ids)
                {
                    plan.Existence[id] = existing == null || existing.ContainsKey(id);
                }

                // The audit does not always keep the name of the related record: it is read with the existence check
                var primaryName = metadata.GetSummary(group.Key)?.PrimaryNameAttribute;
                Entity current;
                foreach (var association in group.Where(a => string.IsNullOrEmpty(a.Target.Name)))
                {
                    if (primaryName != null && existing != null && existing.TryGetValue(association.Target.Id, out current))
                    {
                        association.Target.Name = current.GetAttributeValue<string>(primaryName);
                    }
                }
            }

            bool exists;
            foreach (var association in targets)
            {
                association.Available = plan.ById.ContainsKey(association.Target.Id)
                    || plan.Existence.TryGetValue(association.Target.Id, out exists) && exists;
            }
        }

        /// <summary>
        /// Finds the records that were not deleted with the given items but whose lookup to them was emptied
        /// by the deletion ("Remove link" relationships), and that are still not linked to anything
        /// </summary>
        private void FindUnlinkedRecords(RecoveryPlan plan, TimeSpan window)
        {
            var candidates = new Dictionary<string, Tuple<RecoveryItem, RelinkCandidate>>();

            foreach (var group in plan.Items.GroupBy(i => i.Record.EntityLogicalName))
            {
                var entity = metadata.GetFull(group.Key);
                var relationships = (entity.OneToManyRelationships ?? new OneToManyRelationshipMetadata[0])
                    .Where(r => r.CascadeConfiguration != null
                        && r.CascadeConfiguration.Delete == CascadeType.RemoveLink
                        && !string.IsNullOrEmpty(r.ReferencingAttribute)
                        && metadata.GetSummary(r.ReferencingEntity)?.ObjectTypeCode != null)
                    .ToList();
                if (relationships.Count == 0)
                {
                    continue;
                }

                var childTables = relationships.Select(r => r.ReferencingEntity).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var parents = group.ToDictionary(i => i.Record.RecordId);

                foreach (var interval in MergeIntervals(group.Select(i => i.Record.DeletedOn), window))
                {
                    bool truncated;
                    List<AuditChange> changes;
                    try
                    {
                        changes = audit.FindUpdates(childTables, interval.Item1, interval.Item2, MaxChildCandidates, out truncated);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    foreach (var change in changes.Where(c => c.OldValue != null && !plan.ById.ContainsKey(c.Target.Id)))
                    {
                        foreach (var relationship in relationships.Where(r => string.Equals(r.ReferencingEntity, change.Target.LogicalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var attribute = relationship.ReferencingAttribute;
                            var before = change.OldValue.GetAttributeValue<EntityReference>(attribute);
                            var after = change.NewValue?.GetAttributeValue<EntityReference>(attribute);
                            RecoveryItem parent;
                            if (before == null || after != null || !parents.TryGetValue(before.Id, out parent))
                            {
                                continue;
                            }

                            var key = change.Target.Id + "|" + attribute;
                            if (!candidates.ContainsKey(key))
                            {
                                candidates[key] = Tuple.Create(parent, new RelinkCandidate { Record = change.Target, Attribute = attribute });
                            }
                        }
                    }
                }
            }

            // Only records that still exist and that nobody has linked to another record since
            foreach (var table in candidates.Values.GroupBy(c => c.Item2.Record.LogicalName))
            {
                var current = RetrieveCurrent(table.Key, table.Select(c => c.Item2.Record.Id).Distinct().ToList(), table.Select(c => c.Item2.Attribute).Distinct().ToList());
                if (current == null)
                {
                    continue;
                }

                foreach (var candidate in table)
                {
                    Entity record;
                    if (current.TryGetValue(candidate.Item2.Record.Id, out record) && record.GetAttributeValue<EntityReference>(candidate.Item2.Attribute) == null)
                    {
                        if (string.IsNullOrEmpty(candidate.Item2.Record.Name))
                        {
                            var primaryName = metadata.GetSummary(table.Key)?.PrimaryNameAttribute;
                            candidate.Item2.Record.Name = primaryName == null ? null : record.GetAttributeValue<string>(primaryName);
                        }
                        candidate.Item1.Relinks.Add(candidate.Item2);
                    }
                }
            }
        }

        /// <summary>
        /// Current values of some columns of existing records, or null when the table cannot be queried
        /// </summary>
        private Dictionary<Guid, Entity> RetrieveCurrent(string entityLogicalName, IList<Guid> ids, IList<string> columns)
        {
            var summary = metadata.GetSummary(entityLogicalName);
            if (summary?.PrimaryIdAttribute == null)
            {
                return null;
            }

            var result = new Dictionary<Guid, Entity>();
            try
            {
                var columnSet = new ColumnSet(columns.ToArray());
                if (summary.PrimaryNameAttribute != null)
                {
                    columnSet.AddColumn(summary.PrimaryNameAttribute);
                }
                for (var i = 0; i < ids.Count; i += ExistenceBatchSize)
                {
                    var query = new QueryExpression(entityLogicalName) { ColumnSet = columnSet, NoLock = true };
                    query.Criteria.AddCondition(summary.PrimaryIdAttribute, ConditionOperator.In, ids.Skip(i).Take(ExistenceBatchSize).Select(id => (object)id).ToArray());
                    foreach (var entity in service.RetrieveMultiple(query).Entities)
                    {
                        result[entity.Id] = entity;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Finds the records deleted in cascade with the given items: deletions of child tables that happened
        /// around the same time and whose lookup pointed to one of the items
        /// </summary>
        private List<RecoveryItem> FindChildren(RecoveryPlan plan, List<RecoveryItem> wave, TimeSpan window)
        {
            var found = new List<RecoveryItem>();

            foreach (var group in wave.GroupBy(i => i.Record.EntityLogicalName))
            {
                var entity = metadata.GetFull(group.Key);
                var relationships = (entity.OneToManyRelationships ?? new OneToManyRelationshipMetadata[0])
                    .Where(r => r.CascadeConfiguration != null
                        && r.CascadeConfiguration.Delete == CascadeType.Cascade
                        && !string.IsNullOrEmpty(r.ReferencingAttribute)
                        && metadata.GetSummary(r.ReferencingEntity)?.ObjectTypeCode != null)
                    .ToList();
                if (relationships.Count == 0)
                {
                    continue;
                }

                var childTables = relationships.Select(r => r.ReferencingEntity).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var parents = group.ToDictionary(i => i.Record.RecordId);

                foreach (var interval in MergeIntervals(group.Select(i => i.Record.DeletedOn), window))
                {
                    bool truncated;
                    var candidates = audit.Search(childTables, null, interval.Item1, interval.Item2, MaxChildCandidates, out truncated)
                        .Where(c => !plan.ById.ContainsKey(c.RecordId))
                        .ToList();
                    if (truncated)
                    {
                        foreach (var parent in group.Where(i => i.Record.DeletedOn >= interval.Item1 && i.Record.DeletedOn <= interval.Item2))
                        {
                            parent.Warnings.Add(Strings.RelatedSearchTruncated(MaxChildCandidates));
                        }
                    }
                    audit.LoadDetails(candidates, null);

                    foreach (var candidate in candidates.Where(c => c.OldValue != null))
                    {
                        if (plan.ById.ContainsKey(candidate.RecordId))
                        {
                            continue;
                        }

                        foreach (var relationship in relationships.Where(r => string.Equals(r.ReferencingEntity, candidate.EntityLogicalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var lookup = candidate.OldValue.Contains(relationship.ReferencingAttribute)
                                ? candidate.OldValue[relationship.ReferencingAttribute] as EntityReference
                                : null;
                            RecoveryItem parent;
                            if (lookup == null || !parents.TryGetValue(lookup.Id, out parent))
                            {
                                continue;
                            }

                            var child = new RecoveryItem
                            {
                                Record = candidate,
                                Role = RecoveryRole.RelatedChild,
                                Parent = parent,
                                Relation = Strings.RelationDeletedWithParent(relationship.ReferencingAttribute)
                            };
                            parent.Children.Add(child);
                            plan.Add(child);
                            found.Add(child);
                            break;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Checks the records referenced by the lookups of the given items. Those that no longer exist
        /// are searched in the audit history and added to the plan when found.
        /// </summary>
        private List<RecoveryItem> FindMissingParents(RecoveryPlan plan, List<RecoveryItem> wave, Action<string> progress)
        {
            var toCheck = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);
            var requiredBy = new Dictionary<Guid, Tuple<RecoveryItem, string>>();

            foreach (var item in wave)
            {
                foreach (var reference in GetRestorableReferences(item))
                {
                    var id = reference.Value.Id;
                    if (id == item.Record.RecordId || plan.ById.ContainsKey(id) || plan.Existence.ContainsKey(id))
                    {
                        continue;
                    }

                    HashSet<Guid> ids;
                    if (!toCheck.TryGetValue(reference.Value.LogicalName, out ids))
                    {
                        ids = new HashSet<Guid>();
                        toCheck[reference.Value.LogicalName] = ids;
                    }
                    ids.Add(id);

                    if (!requiredBy.ContainsKey(id))
                    {
                        requiredBy[id] = Tuple.Create(item, reference.Key);
                    }
                }
            }

            foreach (var group in toCheck)
            {
                var existing = FindExisting(group.Key, group.Value);
                foreach (var id in group.Value)
                {
                    // When the check is not possible the record is assumed to exist
                    plan.Existence[id] = existing == null || existing.Contains(id);
                }
            }

            var found = new List<RecoveryItem>();
            foreach (var group in toCheck)
            {
                foreach (var id in group.Value.Where(i => !plan.Existence[i]))
                {
                    progress?.Invoke(Strings.SearchingReferencedDeletion(group.Key, id));

                    DeletedRecord deleted;
                    try
                    {
                        deleted = audit.FindLatestDelete(group.Key, id);
                    }
                    catch (Exception)
                    {
                        deleted = null;
                    }
                    if (deleted?.OldValue == null || plan.ById.ContainsKey(id))
                    {
                        continue;
                    }

                    var requirer = requiredBy[id];
                    var parent = new RecoveryItem
                    {
                        Record = deleted,
                        Role = RecoveryRole.MissingParent,
                        Parent = requirer.Item1,
                        Relation = Strings.RelationReferencedBy(requirer.Item2)
                    };
                    requirer.Item1.Children.Add(parent);
                    plan.Add(parent);
                    found.Add(parent);
                }
            }

            return found;
        }

        private void BuildPreview(RecoveryPlan plan, RecoveryItem item)
        {
            var record = item.Record;
            var entity = metadata.GetFull(record.EntityLogicalName);
            var attributes = metadata.GetAttributes(record.EntityLogicalName);

            foreach (var pair in record.OldValue.Attributes)
            {
                AttributeMetadata attribute;
                attributes.TryGetValue(pair.Key, out attribute);
                if (attribute?.AttributeOf != null)
                {
                    continue;
                }

                string reason;
                var preview = new AttributePreview
                {
                    LogicalName = pair.Key,
                    DisplayName = attribute == null ? pair.Key : MetadataCache.GetDisplayName(attribute),
                    Value = ValueFormatter.Format(pair.Value, attribute),
                    WillRestore = IsRestorable(entity, attributes, pair.Key, out reason),
                    Note = reason
                };

                var reference = pair.Value as EntityReference;
                if (preview.WillRestore && reference != null && reference.Id != record.RecordId)
                {
                    bool exists;
                    if (plan.ById.ContainsKey(reference.Id))
                    {
                        preview.Note = Strings.NoteInRecovery;
                    }
                    else if (plan.Existence.TryGetValue(reference.Id, out exists) && !exists)
                    {
                        preview.WillRestore = false;
                        preview.MissingReference = true;
                        preview.Note = Strings.NoteMissingReference;
                        item.Warnings.Add(Strings.WarningMissingReference(pair.Key, ValueFormatter.Format(reference, attribute)));
                    }
                }

                var state = pair.Value as OptionSetValue;
                if (pair.Key == entity.PrimaryIdAttribute)
                {
                    preview.WillRestore = true;
                    preview.Note = Strings.NoteSameGuid;
                }
                else if (pair.Key == "createdon")
                {
                    preview.Note = attributes.ContainsKey("overriddencreatedon")
                        ? Strings.NoteCreatedOnKept
                        : Strings.NoteCreatedOnSystem;
                }
                else if (pair.Key == "statecode" && state != null && state.Value != 0)
                {
                    preview.Note = Strings.NoteStateAfterCreate;
                }

                item.Attributes.Add(preview);
            }

            item.Attributes.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCultureIgnoreCase));

            foreach (var association in item.Associations)
            {
                item.Attributes.Add(new AttributePreview
                {
                    DisplayName = Strings.AssociationLabel(association.Relationship),
                    LogicalName = association.Relationship,
                    Value = ValueFormatter.Format(association.Target, null),
                    WillRestore = association.Available,
                    Note = association.Available ? Strings.NoteAssociation : Strings.NoteAssociationMissing
                });
            }
            foreach (var relink in item.Relinks)
            {
                item.Attributes.Add(new AttributePreview
                {
                    DisplayName = Strings.RelinkLabel(metadata.GetDisplayName(relink.Record.LogicalName)),
                    LogicalName = relink.Attribute,
                    Value = ValueFormatter.Format(relink.Record, null),
                    WillRestore = true,
                    Note = Strings.NoteRelink
                });
            }
        }

        /// <summary>
        /// Sorts the items so that a record is created after the records of the plan it references
        /// </summary>
        private static List<RecoveryItem> Sort(RecoveryPlan plan)
        {
            var sorted = new List<RecoveryItem>();
            var visited = new HashSet<Guid>();

            foreach (var item in plan.Items)
            {
                Visit(plan, item, visited, sorted);
            }
            return sorted;
        }

        private static void Visit(RecoveryPlan plan, RecoveryItem item, HashSet<Guid> visited, List<RecoveryItem> sorted)
        {
            // Marked before visiting the references so that circular references do not loop
            if (!visited.Add(item.Record.RecordId))
            {
                return;
            }

            RecoveryItem dependency;

            // A record deleted in cascade comes after its parent, following the direction of the relationship
            if (item.Role == RecoveryRole.RelatedChild && item.Parent != null && plan.ById.TryGetValue(item.Parent.Record.RecordId, out dependency))
            {
                Visit(plan, dependency, visited, sorted);
            }

            foreach (var reference in item.Record.OldValue.Attributes.Values.OfType<EntityReference>())
            {
                // A lookup to one of its own deleted children (primary contact...) does not put the child first:
                // the parent is created before, and that lookup is set once the child exists
                if (plan.ById.TryGetValue(reference.Id, out dependency) && !(dependency.Role == RecoveryRole.RelatedChild && dependency.Parent == item))
                {
                    Visit(plan, dependency, visited, sorted);
                }
            }
            sorted.Add(item);
        }

        #endregion Plan

        #region Restore

        public void Restore(RecoveryPlan plan, RecoveryOptions options, Action<string> progress)
        {
            var created = new HashSet<Guid>();
            var deferred = new List<Tuple<RecoveryItem, string, EntityReference>>();
            var total = plan.CountToRestore;
            var index = 0;

            foreach (var item in plan.OrderedItems)
            {
                if (!item.Include)
                {
                    Skip(item, Strings.SkipExcluded);
                    continue;
                }
                if (item.AlreadyExists)
                {
                    Skip(item, Strings.SkipAlreadyExists);
                    continue;
                }

                index++;
                progress?.Invoke(Strings.RestoringRecord(index, total, item.Record.EntityLogicalName, item.Record.Name));

                if (item.Role == RecoveryRole.RelatedChild && item.Parent != null && !IsAvailable(plan, item.Parent.Record.RecordId, created))
                {
                    Skip(item, Strings.SkipParentNotRestored);
                    continue;
                }

                try
                {
                    RestoreItem(plan, item, options, created, deferred);
                    item.Status = RecoveryStatus.Restored;
                    item.Record.Restored = true;
                }
                catch (Exception error)
                {
                    item.Status = RecoveryStatus.Failed;
                    item.Messages.Add(error.Message);
                }
            }

            // Lookups pointing to records created later in the run (circular or self references)
            foreach (var lookup in deferred)
            {
                var item = lookup.Item1;
                if (item.Status != RecoveryStatus.Restored)
                {
                    continue;
                }
                if (!created.Contains(lookup.Item3.Id))
                {
                    item.Messages.Add(Strings.LookupNotRestored(lookup.Item2));
                    continue;
                }

                try
                {
                    var update = new Entity(item.Record.EntityLogicalName) { Id = item.Record.RecordId };
                    update[lookup.Item2] = lookup.Item3;
                    Execute(new UpdateRequest { Target = update }, options);
                }
                catch (Exception error)
                {
                    item.Messages.Add(Strings.LookupNotSet(lookup.Item2, error.Message));
                }
            }

            RestoreLinks(plan, options, created);

            foreach (var item in plan.Items.Where(i => i.Status == RecoveryStatus.Restored && i.Messages.Count > 0))
            {
                item.Status = RecoveryStatus.RestoredWithWarnings;
            }
        }

        private void RestoreItem(RecoveryPlan plan, RecoveryItem item, RecoveryOptions options, HashSet<Guid> created, List<Tuple<RecoveryItem, string, EntityReference>> deferred)
        {
            var record = item.Record;
            var entity = metadata.GetFull(record.EntityLogicalName);
            var attributes = metadata.GetAttributes(record.EntityLogicalName);

            var target = new Entity(record.EntityLogicalName) { Id = record.RecordId };
            OptionSetValue state = null;
            OptionSetValue status = null;

            foreach (var pair in record.OldValue.Attributes)
            {
                string reason;
                if (pair.Value == null || !IsRestorable(entity, attributes, pair.Key, out reason))
                {
                    continue;
                }
                if (pair.Key == "statecode")
                {
                    state = pair.Value as OptionSetValue;
                    continue;
                }
                if (pair.Key == "statuscode")
                {
                    status = pair.Value as OptionSetValue;
                    continue;
                }

                var reference = pair.Value as EntityReference;
                if (reference == null)
                {
                    target[pair.Key] = pair.Value;
                    continue;
                }

                var logicalName = ResolveTarget(reference, attributes[pair.Key]);
                if (logicalName == null)
                {
                    item.Messages.Add(Strings.LookupUnknownTable(pair.Key));
                    continue;
                }
                var lookup = new EntityReference(logicalName, reference.Id);

                bool exists;
                RecoveryItem dependency;
                if (created.Contains(reference.Id) || !plan.Existence.TryGetValue(reference.Id, out exists) || exists)
                {
                    target[pair.Key] = lookup;
                }
                else if (plan.ById.TryGetValue(reference.Id, out dependency) && dependency.Include && dependency.Status == RecoveryStatus.Pending)
                {
                    deferred.Add(Tuple.Create(item, pair.Key, lookup));
                }
                else
                {
                    item.Messages.Add(Strings.LookupMissing(pair.Key, logicalName, reference.Id));
                }
            }

            if (options.PreserveCreatedOn && attributes.ContainsKey("overriddencreatedon") && record.OldValue.Contains("createdon") && record.OldValue["createdon"] is DateTime)
            {
                target["overriddencreatedon"] = record.OldValue["createdon"];
            }

            var inactive = state != null && state.Value != 0;
            AttributeMetadata statusAttribute;
            if (!inactive && status != null && attributes.TryGetValue("statuscode", out statusAttribute) && statusAttribute.IsValidForCreate == true)
            {
                target["statuscode"] = status;
            }

            try
            {
                Execute(new CreateRequest { Target = target }, options);
            }
            catch (Exception error) when (IsOwnerError(target, error))
            {
                // Disabled or deleted user, team without business unit...: the record is still worth restoring
                var owner = record.OldValue.GetAttributeValue<EntityReference>("ownerid") ?? target.GetAttributeValue<EntityReference>("ownerid");
                target.Attributes.Remove("ownerid");
                Execute(new CreateRequest { Target = target }, options);
                item.Messages.Add(Strings.OwnerReplaced(string.IsNullOrEmpty(owner.Name) ? owner.Id.ToString() : owner.Name, error.Message));
            }
            created.Add(record.RecordId);

            if (inactive)
            {
                try
                {
                    SetState(record, state, status, options);
                }
                catch (Exception error)
                {
                    item.Messages.Add(Strings.StatusNotRestored(error.Message));
                }
            }
        }

        /// <summary>
        /// Once every record exists: associates the restored records again (N:N) and links back the records
        /// whose lookup was emptied by the deletion
        /// </summary>
        private void RestoreLinks(RecoveryPlan plan, RecoveryOptions options, HashSet<Guid> created)
        {
            // An association between two restored records is known by both: it is created once
            var done = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in plan.OrderedItems.Where(i => i.Status == RecoveryStatus.Restored))
            {
                var source = new EntityReference(item.Record.EntityLogicalName, item.Record.RecordId);

                foreach (var association in item.Associations)
                {
                    var target = association.Target;
                    var name = ValueFormatter.Format(target, null);
                    var first = item.Record.RecordId.CompareTo(target.Id) < 0 ? item.Record.RecordId : target.Id;
                    var second = first == target.Id ? item.Record.RecordId : target.Id;
                    if (!done.Add(association.Relationship + "|" + first + "|" + second))
                    {
                        continue;
                    }

                    if (!IsAvailable(plan, target.Id, created))
                    {
                        item.Messages.Add(Strings.AssociationTargetMissing(association.Relationship, name));
                        continue;
                    }

                    try
                    {
                        Execute(new AssociateRequest
                        {
                            Target = source,
                            Relationship = CreateRelationship(item.Record.EntityLogicalName, association.Relationship),
                            RelatedEntities = new EntityReferenceCollection { new EntityReference(target.LogicalName, target.Id) }
                        }, options);
                    }
                    catch (Exception error)
                    {
                        // Already associated (by a plug-in, or by the other side): nothing is missing
                        if ((error.Message ?? string.Empty).IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            item.Messages.Add(Strings.AssociationFailed(association.Relationship, name, error.Message));
                        }
                    }
                }

                foreach (var relink in item.Relinks)
                {
                    try
                    {
                        var update = new Entity(relink.Record.LogicalName) { Id = relink.Record.Id };
                        update[relink.Attribute] = source;
                        Execute(new UpdateRequest { Target = update }, options);
                    }
                    catch (Exception error)
                    {
                        item.Messages.Add(Strings.RelinkFailed(ValueFormatter.Format(relink.Record, null), relink.Attribute, error.Message));
                    }
                }
            }
        }

        /// <summary>
        /// A relationship between records of the same table needs the role of the record the request starts from
        /// </summary>
        private Relationship CreateRelationship(string entityLogicalName, string schemaName)
        {
            var relationship = new Relationship(schemaName);
            var metadataRelationship = metadata.GetFull(entityLogicalName).ManyToManyRelationships?
                .FirstOrDefault(r => string.Equals(r.SchemaName, schemaName, StringComparison.OrdinalIgnoreCase));
            if (metadataRelationship != null && string.Equals(metadataRelationship.Entity1LogicalName, metadataRelationship.Entity2LogicalName, StringComparison.OrdinalIgnoreCase))
            {
                relationship.PrimaryEntityRole = EntityRole.Referencing;
            }
            return relationship;
        }

        private static bool IsOwnerError(Entity target, Exception error)
        {
            var owner = target.GetAttributeValue<EntityReference>("ownerid");
            if (owner == null)
            {
                return false;
            }

            var message = error.Message ?? string.Empty;
            return message.IndexOf(owner.Id.ToString(), StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("owner", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("disabled", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("business unit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Puts the record back in its former state. Cases and opportunities cannot be closed by an update:
        /// they are closed with their dedicated message, which creates the closing activity Dataverse expects.
        /// </summary>
        private void SetState(DeletedRecord record, OptionSetValue state, OptionSetValue status, RecoveryOptions options)
        {
            var reference = new EntityReference(record.EntityLogicalName, record.RecordId);
            var subject = string.IsNullOrEmpty(record.Name) ? Strings.ClosedBySystem : record.Name;

            if (record.EntityLogicalName == "incident" && state.Value == 1)
            {
                Execute(new CloseIncidentRequest
                {
                    IncidentResolution = new Entity("incidentresolution") { ["subject"] = subject, ["incidentid"] = reference },
                    Status = status ?? new OptionSetValue(5)
                }, options);
                return;
            }

            if (record.EntityLogicalName == "opportunity" && (state.Value == 1 || state.Value == 2))
            {
                var close = new Entity("opportunityclose") { ["subject"] = subject, ["opportunityid"] = reference };
                if (state.Value == 1)
                {
                    Execute(new WinOpportunityRequest { OpportunityClose = close, Status = status ?? new OptionSetValue(3) }, options);
                }
                else
                {
                    Execute(new LoseOpportunityRequest { OpportunityClose = close, Status = status ?? new OptionSetValue(4) }, options);
                }
                return;
            }

            var update = new Entity(record.EntityLogicalName) { Id = record.RecordId };
            update["statecode"] = state;
            if (status != null)
            {
                update["statuscode"] = status;
            }
            Execute(new UpdateRequest { Target = update }, options);
        }

        private void Execute(OrganizationRequest request, RecoveryOptions options)
        {
            if (options.BypassCustomLogic)
            {
                request.Parameters["BypassCustomPluginExecution"] = true;
            }
            service.Execute(request);
        }

        private static void Skip(RecoveryItem item, string reason)
        {
            item.Status = RecoveryStatus.Skipped;
            item.Messages.Add(reason);
        }

        private static bool IsAvailable(RecoveryPlan plan, Guid id, HashSet<Guid> created)
        {
            bool exists;
            return created.Contains(id) || (plan.Existence.TryGetValue(id, out exists) && exists);
        }

        #endregion Restore

        #region Helpers

        private static bool IsRestorable(EntityMetadata entity, Dictionary<string, AttributeMetadata> attributes, string logicalName, out string reason)
        {
            reason = null;

            AttributeMetadata attribute;
            if (!attributes.TryGetValue(logicalName, out attribute))
            {
                reason = Strings.NoteColumnRemoved;
                return false;
            }
            if (attribute.AttributeOf != null)
            {
                reason = Strings.NoteDerived;
                return false;
            }
            if (logicalName == entity.PrimaryIdAttribute)
            {
                reason = Strings.NoteThroughGuid;
                return false;
            }
            if (logicalName == "overriddencreatedon")
            {
                reason = Strings.NoteOverriddenCreatedOn;
                return false;
            }
            if (logicalName == "statecode" || logicalName == "statuscode")
            {
                return true;
            }
            if (attribute.IsValidForCreate != true)
            {
                reason = Strings.NoteSystemColumn;
                return false;
            }
            return true;
        }

        private IEnumerable<KeyValuePair<string, EntityReference>> GetRestorableReferences(RecoveryItem item)
        {
            var entity = metadata.GetFull(item.Record.EntityLogicalName);
            var attributes = metadata.GetAttributes(item.Record.EntityLogicalName);

            foreach (var pair in item.Record.OldValue.Attributes)
            {
                var reference = pair.Value as EntityReference;
                string reason;
                if (reference == null || reference.Id == Guid.Empty || !IsRestorable(entity, attributes, pair.Key, out reason))
                {
                    continue;
                }

                var logicalName = ResolveTarget(reference, attributes[pair.Key]);
                if (logicalName != null)
                {
                    yield return new KeyValuePair<string, EntityReference>(pair.Key, new EntityReference(logicalName, reference.Id));
                }
            }
        }

        private static string ResolveTarget(EntityReference reference, AttributeMetadata attribute)
        {
            if (!string.IsNullOrEmpty(reference.LogicalName))
            {
                return reference.LogicalName;
            }

            var targets = (attribute as LookupAttributeMetadata)?.Targets;
            return targets != null && targets.Length == 1 ? targets[0] : null;
        }

        /// <summary>
        /// Returns the ids of the records that exist, or null when the table cannot be queried
        /// </summary>
        public HashSet<Guid> FindExisting(string entityLogicalName, ICollection<Guid> ids)
        {
            var primaryId = metadata.GetSummary(entityLogicalName)?.PrimaryIdAttribute;
            if (primaryId == null)
            {
                return null;
            }

            var existing = new HashSet<Guid>();
            try
            {
                var remaining = ids.ToList();
                for (var i = 0; i < remaining.Count; i += ExistenceBatchSize)
                {
                    var query = new QueryExpression(entityLogicalName)
                    {
                        ColumnSet = new ColumnSet(primaryId),
                        NoLock = true
                    };
                    query.Criteria.AddCondition(primaryId, ConditionOperator.In, remaining.Skip(i).Take(ExistenceBatchSize).Select(id => (object)id).ToArray());

                    foreach (var entity in service.RetrieveMultiple(query).Entities)
                    {
                        existing.Add(entity.Id);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return existing;
        }

        /// <summary>
        /// Builds the time ranges to search around the given deletion dates, merging those that overlap
        /// </summary>
        private static List<Tuple<DateTime, DateTime>> MergeIntervals(IEnumerable<DateTime> dates, TimeSpan window)
        {
            var result = new List<Tuple<DateTime, DateTime>>();
            foreach (var date in dates.OrderBy(d => d))
            {
                var from = date - window;
                var to = date + window;
                var last = result.LastOrDefault();
                if (last != null && from <= last.Item2)
                {
                    result[result.Count - 1] = Tuple.Create(last.Item1, to);
                }
                else
                {
                    result.Add(Tuple.Create(from, to));
                }
            }
            return result;
        }

        #endregion Helpers
    }
}
