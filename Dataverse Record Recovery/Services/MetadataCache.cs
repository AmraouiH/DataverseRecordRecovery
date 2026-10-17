using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Dataverse_Record_Recovery.Services
{
    /// <summary>
    /// Loads table metadata on demand and keeps it for the lifetime of the connection
    /// </summary>
    public class MetadataCache
    {
        private readonly IOrganizationService service;
        private readonly object sync = new object();
        private readonly Dictionary<string, EntityMetadata> summaries = new Dictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EntityMetadata> full = new Dictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, AttributeMetadata>> attributes = new Dictionary<string, Dictionary<string, AttributeMetadata>>(StringComparer.OrdinalIgnoreCase);

        public MetadataCache(IOrganizationService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Loads the list of tables (without columns nor relationships)
        /// </summary>
        public void LoadEntities()
        {
            var response = (RetrieveAllEntitiesResponse)service.Execute(new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity
            });

            lock (sync)
            {
                summaries.Clear();
                foreach (var entity in response.EntityMetadata)
                {
                    summaries[entity.LogicalName] = entity;
                }
            }
        }

        public List<EntityMetadata> Entities
        {
            get
            {
                lock (sync)
                {
                    return summaries.Values.ToList();
                }
            }
        }

        public EntityMetadata GetSummary(string logicalName)
        {
            lock (sync)
            {
                EntityMetadata result;
                return logicalName != null && summaries.TryGetValue(logicalName, out result) ? result : null;
            }
        }

        /// <summary>
        /// Returns the table metadata with columns and relationships only if already loaded
        /// </summary>
        public EntityMetadata TryGetFull(string logicalName)
        {
            lock (sync)
            {
                EntityMetadata result;
                return logicalName != null && full.TryGetValue(logicalName, out result) ? result : null;
            }
        }

        /// <summary>
        /// Returns the table metadata with columns and relationships, calling the server when needed
        /// </summary>
        public EntityMetadata GetFull(string logicalName)
        {
            var cached = TryGetFull(logicalName);
            if (cached != null)
            {
                return cached;
            }

            var response = (RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest
            {
                LogicalName = logicalName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships
            });

            lock (sync)
            {
                full[logicalName] = response.EntityMetadata;
                attributes[logicalName] = (response.EntityMetadata.Attributes ?? new AttributeMetadata[0])
                    .GroupBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }

            return response.EntityMetadata;
        }

        public Dictionary<string, AttributeMetadata> GetAttributes(string logicalName)
        {
            GetFull(logicalName);
            return TryGetAttributes(logicalName);
        }

        public Dictionary<string, AttributeMetadata> TryGetAttributes(string logicalName)
        {
            lock (sync)
            {
                Dictionary<string, AttributeMetadata> result;
                return logicalName != null && attributes.TryGetValue(logicalName, out result) ? result : null;
            }
        }

        public string GetDisplayName(string logicalName)
        {
            var summary = GetSummary(logicalName);
            return summary == null ? logicalName : GetDisplayName(summary);
        }

        public static string GetDisplayName(EntityMetadata entity)
        {
            return entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName;
        }

        public static string GetDisplayName(AttributeMetadata attribute)
        {
            return attribute.DisplayName?.UserLocalizedLabel?.Label ?? attribute.LogicalName;
        }

        public static bool IsAudited(EntityMetadata entity)
        {
            return entity.IsAuditEnabled != null && entity.IsAuditEnabled.Value;
        }
    }
}
