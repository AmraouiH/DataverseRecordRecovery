namespace Dataverse_Record_Recovery
{
    /// <summary>
    /// This class can help you to store settings for your plugin
    /// </summary>
    /// <remarks>
    /// This class must be XML serializable
    /// </remarks>
    public class Settings
    {
        public string LastUsedOrganizationWebappUrl { get; set; }

        public string LastTable { get; set; }

        public bool AuditedTablesOnly { get; set; } = true;

        public int MaxRecords { get; set; } = 500;

        public bool IncludeRelated { get; set; } = true;

        public int RelatedWindowSeconds { get; set; } = 120;

        public bool PreserveCreatedOn { get; set; } = true;

        /// <summary>Language of the tool: "en" (default) or "fr"</summary>
        public string Language { get; set; } = "en";
    }
}
