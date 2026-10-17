using System;
using System.Collections.Generic;
using System.Linq;
using Dataverse_Record_Recovery.Localization;
using Dataverse_Record_Recovery.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Dataverse_Record_Recovery.Services
{
    /// <summary>
    /// Change of a record read from the audit history: column values before and after an update
    /// </summary>
    public class AuditChange
    {
        public EntityReference Target { get; set; }

        public DateTime ChangedOn { get; set; }

        public Entity OldValue { get; set; }

        public Entity NewValue { get; set; }
    }

    /// <summary>
    /// Reads record deletions, updates and N:N associations from the audit history
    /// </summary>
    public class AuditService
    {
        private const int OperationUpdate = 2;
        private const int OperationDelete = 3;
        private const int ActionAssociate = 33;
        private const int ActionDisassociate = 34;
        private const int PageSize = 500;
        private const int DetailsBatchSize = 50;

        /// <summary>Disassociations logged this close to a deletion are caused by it</summary>
        private static readonly TimeSpan DeletionMargin = TimeSpan.FromSeconds(5);

        private readonly IOrganizationService service;
        private readonly MetadataCache metadata;
        private bool useBatchForDetails = true;

        public AuditService(IOrganizationService service, MetadataCache metadata)
        {
            this.service = service;
            this.metadata = metadata;
        }

        /// <summary>
        /// Finds the deletions of records of the given tables, most recent first. Audit details are not loaded.
        /// </summary>
        public List<DeletedRecord> Search(IEnumerable<string> entityLogicalNames, Guid? userId, DateTime fromUtc, DateTime toUtc, int maxRecords, out bool truncated)
        {
            truncated = false;
            var query = CreateTableQuery(OperationDelete, entityLogicalNames, fromUtc, toUtc);
            if (query == null)
            {
                return new List<DeletedRecord>();
            }
            if (userId.HasValue)
            {
                query.Criteria.AddCondition("userid", ConditionOperator.Equal, userId.Value);
            }

            return RetrieveAll(query, maxRecords, out truncated)
                .Select(ToDeletedRecord)
                .Where(r => r != null)
                .ToList();
        }

        /// <summary>
        /// Finds the updates of records of the given tables in a period, with their values before and after
        /// </summary>
        public List<AuditChange> FindUpdates(IEnumerable<string> entityLogicalNames, DateTime fromUtc, DateTime toUtc, int maxRecords, out bool truncated)
        {
            truncated = false;
            var query = CreateTableQuery(OperationUpdate, entityLogicalNames, fromUtc, toUtc);
            if (query == null)
            {
                return new List<AuditChange>();
            }

            var audits = RetrieveAll(query, maxRecords, out truncated);
            var details = RetrieveDetails(audits.Select(a => a.Id).ToList(), null);

            var result = new List<AuditChange>();
            foreach (var audit in audits)
            {
                object detail;
                var attributeDetail = details.TryGetValue(audit.Id, out detail) ? detail as AttributeAuditDetail : null;
                var target = audit.GetAttributeValue<EntityReference>("objectid");
                if (attributeDetail == null || target == null)
                {
                    continue;
                }
                result.Add(new AuditChange
                {
                    Target = target,
                    ChangedOn = audit.GetAttributeValue<DateTime>("createdon"),
                    OldValue = attributeDetail.OldValue,
                    NewValue = attributeDetail.NewValue
                });
            }
            return result;
        }

        /// <summary>
        /// Rebuilds the N:N associations a record had just before its deletion, by replaying the associations
        /// and disassociations of its audit history. Returns the related records by relationship name.
        /// </summary>
        public Dictionary<string, List<EntityReference>> LoadAssociations(Guid recordId, DateTime deletedOnUtc)
        {
            var query = new QueryExpression("audit")
            {
                ColumnSet = new ColumnSet("createdon", "action"),
                NoLock = true
            };
            query.Criteria.AddCondition("objectid", ConditionOperator.Equal, recordId);
            query.Criteria.AddCondition("action", ConditionOperator.In, ActionAssociate, ActionDisassociate);
            query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, deletedOnUtc + DeletionMargin);
            query.AddOrder("createdon", OrderType.Ascending);
            query.PageInfo = new PagingInfo { Count = PageSize, PageNumber = 1 };

            bool truncated;
            // Replayed in chronological order, whatever order the server returns
            var audits = RetrieveAll(query, int.MaxValue, out truncated).OrderBy(a => a.GetAttributeValue<DateTime>("createdon")).ToList();
            var details = RetrieveDetails(audits.Select(a => a.Id).ToList(), null);

            var current = new Dictionary<string, Dictionary<Guid, EntityReference>>(StringComparer.OrdinalIgnoreCase);
            foreach (var audit in audits)
            {
                object value;
                var detail = details.TryGetValue(audit.Id, out value) ? value as RelationshipAuditDetail : null;
                if (detail?.TargetRecords == null || string.IsNullOrEmpty(detail.RelationshipName))
                {
                    continue;
                }

                var associate = audit.GetAttributeValue<OptionSetValue>("action")?.Value == ActionAssociate;
                var when = audit.GetAttributeValue<DateTime>("createdon");
                if (!associate && when >= deletedOnUtc - DeletionMargin)
                {
                    // Removed by the deletion itself: this is what has to be restored
                    continue;
                }

                Dictionary<Guid, EntityReference> targets;
                if (!current.TryGetValue(detail.RelationshipName, out targets))
                {
                    targets = new Dictionary<Guid, EntityReference>();
                    current[detail.RelationshipName] = targets;
                }
                foreach (var target in detail.TargetRecords.Where(t => t != null && t.Id != Guid.Empty))
                {
                    if (associate)
                    {
                        targets[target.Id] = target;
                    }
                    else
                    {
                        targets.Remove(target.Id);
                    }
                }
            }

            return current
                .Where(p => p.Value.Count > 0)
                .ToDictionary(p => p.Key, p => p.Value.Values.ToList(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds the most recent deletion of a given record, with its audit details. Returns null when there is none.
        /// </summary>
        public DeletedRecord FindLatestDelete(string entityLogicalName, Guid recordId)
        {
            var query = CreateQuery(OperationDelete);
            query.Criteria.AddCondition("objectid", ConditionOperator.Equal, recordId);
            query.TopCount = 1;

            var audit = service.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (audit == null)
            {
                return null;
            }

            var record = ToDeletedRecord(audit);
            if (record == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(record.EntityLogicalName))
            {
                record.EntityLogicalName = entityLogicalName;
            }
            LoadDetails(new[] { record }, null);
            return record;
        }

        /// <summary>
        /// Loads the column values the records had before their deletion
        /// </summary>
        public void LoadDetails(IList<DeletedRecord> records, Action<int, int> progress)
        {
            var pending = records.Where(r => r.OldValue == null).ToList();
            var details = RetrieveDetails(pending.Select(r => r.AuditId).ToList(), progress);

            foreach (var record in pending)
            {
                object detail;
                if (!details.TryGetValue(record.AuditId, out detail))
                {
                    record.Error = Strings.NoAuditDetail;
                }
                else if (detail is string)
                {
                    record.Error = (string)detail;
                }
                else
                {
                    ApplyDetail(record, detail as AuditDetail);
                }
            }
        }

        /// <summary>
        /// Returns the detail of each audit entry: an AuditDetail, or the error message (string) when it could not be read
        /// </summary>
        private Dictionary<Guid, object> RetrieveDetails(IList<Guid> auditIds, Action<int, int> progress)
        {
            var result = new Dictionary<Guid, object>();
            for (var i = 0; i < auditIds.Count; i += DetailsBatchSize)
            {
                progress?.Invoke(i, auditIds.Count);
                var chunk = auditIds.Skip(i).Take(DetailsBatchSize).ToList();

                if (useBatchForDetails && chunk.Count > 1)
                {
                    try
                    {
                        RetrieveDetailsBatch(chunk, result);
                        continue;
                    }
                    catch (Exception)
                    {
                        // Batches not accepted for this message: load the details one by one from now on
                        useBatchForDetails = false;
                    }
                }

                foreach (var id in chunk)
                {
                    try
                    {
                        result[id] = ((RetrieveAuditDetailsResponse)service.Execute(new RetrieveAuditDetailsRequest { AuditId = id })).AuditDetail;
                    }
                    catch (Exception error)
                    {
                        result[id] = error.Message;
                    }
                }
            }
            progress?.Invoke(auditIds.Count, auditIds.Count);
            return result;
        }

        private void RetrieveDetailsBatch(List<Guid> chunk, Dictionary<Guid, object> result)
        {
            var request = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings { ContinueOnError = true, ReturnResponses = true },
                Requests = new OrganizationRequestCollection()
            };
            foreach (var id in chunk)
            {
                request.Requests.Add(new RetrieveAuditDetailsRequest { AuditId = id });
            }

            var response = (ExecuteMultipleResponse)service.Execute(request);
            foreach (var item in response.Responses)
            {
                var id = chunk[item.RequestIndex];
                if (item.Fault != null)
                {
                    result[id] = item.Fault.Message;
                }
                else if (item.Response != null && item.Response.Results.Contains("AuditDetail"))
                {
                    result[id] = item.Response.Results["AuditDetail"];
                }
            }
        }

        private void ApplyDetail(DeletedRecord record, AuditDetail detail)
        {
            var attributeDetail = detail as AttributeAuditDetail;
            if (attributeDetail == null)
            {
                record.Error = Strings.AuditWithoutValues;
                return;
            }

            record.OldValue = attributeDetail.OldValue ?? new Entity(record.EntityLogicalName);
            record.Error = null;

            var primaryName = metadata.GetSummary(record.EntityLogicalName)?.PrimaryNameAttribute;
            if (primaryName != null && record.OldValue.Contains(primaryName))
            {
                var name = record.OldValue[primaryName] as string;
                if (!string.IsNullOrEmpty(name))
                {
                    record.Name = name;
                }
            }
        }

        /// <summary>
        /// Query on the audit entries of an operation for some tables in a period, or null when no table can be queried
        /// </summary>
        private QueryExpression CreateTableQuery(int operation, IEnumerable<string> entityLogicalNames, DateTime fromUtc, DateTime toUtc)
        {
            var typeCodes = entityLogicalNames
                .Select(n => metadata.GetSummary(n))
                .Where(m => m?.ObjectTypeCode != null)
                .Select(m => (object)m.ObjectTypeCode.Value)
                .ToArray();
            if (typeCodes.Length == 0)
            {
                return null;
            }

            var query = CreateQuery(operation);
            query.Criteria.AddCondition("objecttypecode", ConditionOperator.In, typeCodes);
            query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, fromUtc);
            query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, toUtc);
            query.PageInfo = new PagingInfo { Count = PageSize, PageNumber = 1 };
            return query;
        }

        private List<Entity> RetrieveAll(QueryExpression query, int maxRecords, out bool truncated)
        {
            truncated = false;
            var result = new List<Entity>();
            while (true)
            {
                var page = service.RetrieveMultiple(query);
                foreach (var audit in page.Entities)
                {
                    if (result.Count >= maxRecords)
                    {
                        truncated = true;
                        return result;
                    }
                    result.Add(audit);
                }

                if (!page.MoreRecords)
                {
                    return result;
                }
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }
        }

        private static QueryExpression CreateQuery(int operation)
        {
            var query = new QueryExpression("audit")
            {
                ColumnSet = new ColumnSet("createdon", "objectid", "objecttypecode", "userid"),
                NoLock = true
            };
            query.Criteria.AddCondition("operation", ConditionOperator.Equal, operation);
            query.AddOrder("createdon", OrderType.Descending);
            return query;
        }

        private static DeletedRecord ToDeletedRecord(Entity audit)
        {
            var target = audit.GetAttributeValue<EntityReference>("objectid");
            if (target == null || target.Id == Guid.Empty)
            {
                return null;
            }

            var logicalName = target.LogicalName;
            if (string.IsNullOrEmpty(logicalName))
            {
                logicalName = audit.GetAttributeValue<string>("objecttypecode");
            }

            return new DeletedRecord
            {
                AuditId = audit.Id,
                EntityLogicalName = logicalName,
                RecordId = target.Id,
                DeletedOn = audit.GetAttributeValue<DateTime>("createdon"),
                DeletedBy = audit.GetAttributeValue<EntityReference>("userid"),
                Name = target.Name
            };
        }
    }
}
