using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Dataverse_Record_Recovery
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Dataverse Record Recovery"),
        ExportMetadata("Description", "Find deleted records in the audit history by table, user and date range, then restore them with their original GUID together with their deleted related records"),
        ExportMetadata("SmallImageBase64", PluginIcons.Small),
        ExportMetadata("BigImageBase64", PluginIcons.Big),
        ExportMetadata("BackgroundColor", "orange"),
        ExportMetadata("PrimaryFontColor", "white"),
        ExportMetadata("SecondaryFontColor", "Black")]
    public class RecordRecoveryPlugin : PluginBase, IPayPalPlugin
    {
        /// <summary>Author contact and donation links (same accounts as Entity Fields Analyser).</summary>
        internal const string AuthorEmail = "hamzamraoui11@gmail.com";
        internal const string LinkedInUrl = "https://www.linkedin.com/in/hamza-amraoui/";
        internal const string DonateUrl = "https://www.paypal.com/paypalme/EntityFieldsAnalyser";

        // IPayPalPlugin must be implemented by the plugin class (not the control) for XrmToolBox to show the donation block.
        public string DonationDescription => "Dataverse Record Recovery";

        public string EmailAccount => AuthorEmail;

        public override IXrmToolBoxPluginControl GetControl()
        {
            return new RecordRecoveryControl();
        }
    }
}
