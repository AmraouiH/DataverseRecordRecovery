using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Dataverse_Record_Recovery.Models
{
    /// <summary>
    /// A record deletion found in the audit history
    /// </summary>
    public class DeletedRecord
    {
        public Guid AuditId { get; set; }

        public string EntityLogicalName { get; set; }

        public Guid RecordId { get; set; }

        /// <summary>Deletion date (UTC)</summary>
        public DateTime DeletedOn { get; set; }

        public EntityReference DeletedBy { get; set; }

        /// <summary>Column values of the record just before its deletion, null until the audit details are loaded</summary>
        public Entity OldValue { get; set; }

        public string Name { get; set; }

        /// <summary>Reason why the audit details could not be loaded</summary>
        public string Error { get; set; }

        public bool Restored { get; set; }
    }

    public enum RecoveryRole
    {
        /// <summary>Record picked by the user</summary>
        Selected,

        /// <summary>Deleted record referenced by a lookup of a record to restore</summary>
        MissingParent,

        /// <summary>Record deleted in cascade with a record to restore</summary>
        RelatedChild
    }

    public enum RecoveryStatus
    {
        Pending,
        Restored,
        RestoredWithWarnings,
        Skipped,
        Failed
    }

    public class AttributePreview
    {
        public string LogicalName { get; set; }

        public string DisplayName { get; set; }

        public string Value { get; set; }

        public bool WillRestore { get; set; }

        public string Note { get; set; }

        /// <summary>Lookup to a record that no longer exists and cannot be restored</summary>
        public bool MissingReference { get; set; }
    }

    /// <summary>
    /// N:N association the record had before its deletion
    /// </summary>
    public class AssociationLink
    {
        public string Relationship { get; set; }

        public EntityReference Target { get; set; }

        /// <summary>The related record exists, or is part of the recovery</summary>
        public bool Available { get; set; }
    }

    /// <summary>
    /// Record that was not deleted with the record but whose lookup was emptied by the deletion ("Remove link" relationship)
    /// </summary>
    public class RelinkCandidate
    {
        public EntityReference Record { get; set; }

        public string Attribute { get; set; }
    }

    public class RecoveryItem
    {
        public DeletedRecord Record { get; set; }

        public RecoveryRole Role { get; set; }

        /// <summary>Item that brought this one into the plan (null for selected records)</summary>
        public RecoveryItem Parent { get; set; }

        public List<RecoveryItem> Children { get; } = new List<RecoveryItem>();

        /// <summary>How this item relates to its parent item</summary>
        public string Relation { get; set; }

        public bool AlreadyExists { get; set; }

        public bool Include { get; set; } = true;

        /// <summary>Problems detected while building the plan</summary>
        public List<string> Warnings { get; } = new List<string>();

        public List<AttributePreview> Attributes { get; } = new List<AttributePreview>();

        public List<AssociationLink> Associations { get; } = new List<AssociationLink>();

        public List<RelinkCandidate> Relinks { get; } = new List<RelinkCandidate>();

        public RecoveryStatus Status { get; set; }

        /// <summary>Problems met while restoring</summary>
        public List<string> Messages { get; } = new List<string>();

        public string Message => string.Join(" | ", Messages);
    }

    public class RecoveryPlan
    {
        /// <summary>Records picked by the user</summary>
        public List<RecoveryItem> Roots { get; } = new List<RecoveryItem>();

        public List<RecoveryItem> Items { get; } = new List<RecoveryItem>();

        /// <summary>Items sorted so that referenced records are created before the records referencing them</summary>
        public List<RecoveryItem> OrderedItems { get; set; } = new List<RecoveryItem>();

        public Dictionary<Guid, RecoveryItem> ById { get; } = new Dictionary<Guid, RecoveryItem>();

        /// <summary>Records whose existence in the environment has been checked</summary>
        public Dictionary<Guid, bool> Existence { get; } = new Dictionary<Guid, bool>();

        /// <summary>Records of the plan that already exist in the environment and are therefore left out</summary>
        public int ExistingCount { get; set; }

        public void Add(RecoveryItem item)
        {
            Items.Add(item);
            ById[item.Record.RecordId] = item;
        }

        public void Remove(RecoveryItem item)
        {
            Items.Remove(item);
            Roots.Remove(item);
            ById.Remove(item.Record.RecordId);
        }

        public int CountToRestore => Items.Count(i => i.Include && !i.AlreadyExists);
    }

    public class RecoveryOptions
    {
        /// <summary>Set overriddencreatedon with the original creation date</summary>
        public bool PreserveCreatedOn { get; set; }

        /// <summary>Do not run custom plug-ins and flows while restoring</summary>
        public bool BypassCustomLogic { get; set; }
    }
}
