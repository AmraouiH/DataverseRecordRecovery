using System;

namespace Dataverse_Record_Recovery.Localization
{
    public enum AppLanguage
    {
        English,
        French
    }

    /// <summary>
    /// Every text shown by the tool, in English and French. English is the default language.
    /// </summary>
    internal static class Strings
    {
        public static AppLanguage Current { get; set; } = AppLanguage.English;

        public static string Code => Current == AppLanguage.French ? "fr" : "en";

        public static AppLanguage FromCode(string code)
        {
            return string.Equals(code, "fr", StringComparison.OrdinalIgnoreCase) ? AppLanguage.French : AppLanguage.English;
        }

        public static string NativeName(AppLanguage language)
        {
            return language == AppLanguage.French ? "Français" : "English";
        }

        private static string L(string en, string fr)
        {
            return Current == AppLanguage.French ? fr : en;
        }

        private static string F(string en, string fr, params object[] args)
        {
            return string.Format(L(en, fr), args);
        }

        #region Header

        public static string HeaderSubtitle => L(
            "Restore deleted Dataverse records from the audit history, with their original GUID",
            "Restaurez les enregistrements supprimés depuis l'audit, avec leur GUID d'origine");
        public static string Close => L("Close", "Fermer");
        public static string Donate => L("Donate", "Faire un don");
        public static string DonateTip => L("Support the tool (PayPal)", "Soutenir l'outil (PayPal)");
        public static string AuthorTip => L("Developed by Hamza AMRAOUI (LinkedIn)", "Développé par Hamza AMRAOUI (LinkedIn)");
        public static string ReloadTables => L("Reload tables", "Recharger les tables");
        public static string ReloadTablesTip => L("Reload the tables and users of the connected environment", "Recharger les tables et les utilisateurs de l'environnement connecté");
        public static string LanguageTip => L("Language of the tool", "Langue de l'outil");
        public static string NotConnected => L("○ Not connected", "○ Non connecté");
        public static string ConnectedTo(string name) => F("● Connected to {0}", "● Connecté à {0}", name);

        #endregion

        #region Step 1: filters

        public static string FiltersTitle => L("Find deleted records", "Rechercher les enregistrements supprimés");
        public static string FiltersSubtitle => L(
            "Choose a table, who deleted the records and when (local time), then search the audit history",
            "Choisissez une table, qui a supprimé les enregistrements et quand (heure locale), puis interrogez l'historique d'audit");
        public static string Table => L("Table", "Table");
        public static string DeletedBy => L("Deleted by", "Supprimé par");
        public static string DeletedFrom => L("Deleted from", "Supprimé du");
        public static string To => L("To", "Au");
        public static string MaxRecords => L("Max records", "Nombre max.");
        public static string Search => L("Search", "Rechercher");
        public static string AuditedOnly => L("Only tables with auditing on", "Uniquement les tables auditées");
        public static string AuditedOnlyTip => L(
            "Untick to also list the tables whose auditing is now disabled",
            "Décochez pour lister aussi les tables dont l'audit est désactivé aujourd'hui");
        public static string IncludeRelated => L(
            "Also restore related records deleted in cascade, within",
            "Restaurer aussi les enregistrements liés supprimés en cascade, à");
        public static string WindowSuffix => L("seconds before or after each deletion", "secondes près de chaque suppression");
        public static string WindowTip => L(
            "Related records are searched in the audit history around the deletion of each record to restore",
            "Les enregistrements liés sont recherchés dans l'historique d'audit autour de la suppression de chaque enregistrement à restaurer");
        public static string MaxRecordsTip => L(
            "Maximum number of deleted records loaded by a search",
            "Nombre maximum d'enregistrements supprimés chargés par une recherche");
        public static string AllUsers => L("(All users)", "(Tous les utilisateurs)");
        public static string Disabled => L("disabled", "désactivé");

        #endregion

        #region Step 2: records

        public static string RecordsTitle => L("Select the records to restore", "Sélectionnez les enregistrements à restaurer");
        public static string RecordsSubtitle => L(
            "Check the records to restore. Click a row to see its values before deletion",
            "Cochez les enregistrements à restaurer. Cliquez sur une ligne pour voir ses valeurs avant suppression");
        public static string CheckAll => L("Check all", "Tout cocher");
        public static string UncheckAll => L("Uncheck all", "Tout décocher");
        public static string ColDeletedRecord => L("DELETED RECORD", "ENREGISTREMENT SUPPRIMÉ");
        public static string ColGuid => "GUID";
        public static string ColDeletedOn => L("DELETED ON", "SUPPRIMÉ LE");
        public static string ColDeletedBy => L("DELETED BY", "SUPPRIMÉ PAR");
        public static string NoName => L("(no name)", "(sans nom)");
        public static string StatusCannotRestore(string error) => F("Cannot be restored: {0}", "Restauration impossible : {0}", error);
        public static string DefaultSummary => L("No search yet: fill in step 1 and click Search.", "Aucune recherche : complétez l'étape 1 puis cliquez sur Rechercher.");
        public static string SummaryChecked(string summary, int count) => F("{0}  ·  {1} checked", "{0}  ·  {1} coché(s)", summary, count);
        public static string PreviewAndRestore => L("Preview and restore", "Prévisualiser et restaurer");
        public static string PreviewAndRestoreCount(int count) => F("Preview and restore {0} record(s)", "Prévisualiser et restaurer {0} enregistrement(s)", count);
        public static string NoRecordFound(string table) => F("No deleted record found in table {0} for these filters.", "Aucun enregistrement supprimé trouvé dans la table {0} pour ces filtres.", table);
        public static string SearchTruncated(int count) => F(
            "First {0} deleted record(s) shown, the limit is reached: narrow the filters or raise the maximum.",
            "Les {0} premiers enregistrements supprimés sont affichés, la limite est atteinte : affinez les filtres ou augmentez le maximum.", count);
        public static string ExistingHidden(int count) => F(
            "{0} record(s) that exist again are hidden.",
            "{0} enregistrement(s) qui existent de nouveau sont masqués.", count);
        public static string RecordsFound(int count, string table) => F("{0} deleted record(s) found in table {1}.", "{0} enregistrement(s) supprimé(s) trouvé(s) dans la table {1}.", count, table);

        #endregion

        #region Details and log

        public static string DetailsTitle => L("Values before deletion", "Valeurs avant suppression");
        public static string DetailsSubtitle => L(
            "Click a deleted record to see what the audit history kept",
            "Cliquez sur un enregistrement supprimé pour voir ce que l'historique d'audit a conservé");
        public static string ColColumn => L("COLUMN", "COLONNE");
        public static string ColLogicalName => L("LOGICAL NAME", "NOM LOGIQUE");
        public static string ColValue => L("VALUE", "VALEUR");
        public static string LogTitle => L("Recovery log", "Journal de restauration");
        public static string LogSubtitle => L(
            "Result of the last recovery, parents first. Double-click a row to read its full message",
            "Résultat de la dernière restauration, parents en premier. Double-cliquez une ligne pour lire son message complet");
        public static string ColResult => L("RESULT", "RÉSULTAT");
        public static string ColRole => L("ROLE", "RÔLE");
        public static string ColTable => L("TABLE", "TABLE");
        public static string ColRecord => L("RECORD", "ENREGISTREMENT");
        public static string ColMessage => L("MESSAGE", "MESSAGE");

        public static string RoleSelected => L("Selected", "Sélectionné");
        public static string RoleMissingParent => L("Missing parent", "Parent manquant");
        public static string RoleRelated => L("Related", "Lié");

        public static string ResultRestored => L("Restored", "Restauré");
        public static string ResultRestoredWithWarnings => L("Restored with warnings", "Restauré avec avertissements");
        public static string ResultSkipped => L("Skipped", "Ignoré");
        public static string ResultFailed => L("Failed", "Échec");
        public static string ResultNotProcessed => L("Not processed", "Non traité");

        #endregion

        #region Work messages

        public static string LoadingTables => L("Loading tables...", "Chargement des tables...");
        public static string LoadingUsers => L("Loading users...", "Chargement des utilisateurs...");
        public static string SearchingDeleted => L("Searching deleted records in the audit history...", "Recherche des enregistrements supprimés dans l'historique d'audit...");
        public static string LoadingDetails(int done, int total) => F(
            "Loading the details of the deleted records ({0}/{1})...",
            "Chargement du détail des enregistrements supprimés ({0}/{1})...", done, total);
        public static string Analyzing => L("Analyzing the records to restore...", "Analyse des enregistrements à restaurer...");
        public static string Restoring => L("Restoring records...", "Restauration des enregistrements...");
        public static string CurrentEnvironment => L("current environment", "environnement actuel");

        #endregion

        #region Messages

        public static string AuditDisabled => L(
            "Auditing is currently disabled in this environment: only records deleted while auditing was enabled can be found.",
            "L'audit est actuellement désactivé dans cet environnement : seuls les enregistrements supprimés pendant que l'audit était actif peuvent être retrouvés.");
        public static string SelectTableTitle => L("Select a table", "Sélectionnez une table");
        public static string SelectTableMessage => L(
            "Choose in step 1 the table whose deleted records you are looking for.",
            "Choisissez à l'étape 1 la table dont vous recherchez les enregistrements supprimés.");
        public static string UnknownUserTitle => L("Unknown user", "Utilisateur inconnu");
        public static string UnknownUserMessage => L(
            "Select a user in the \"Deleted by\" list, or leave it on \"(All users)\".",
            "Sélectionnez un utilisateur dans la liste « Supprimé par », ou laissez « (Tous les utilisateurs) ».");
        public static string DateRangeTitle => L("Check the date range", "Vérifiez la période");
        public static string DateRangeMessage => L(
            "The \"Deleted from\" date must be before the \"To\" date.",
            "La date « Supprimé du » doit précéder la date « Au ».");
        public static string NoRecordCheckedTitle => L("No record checked", "Aucun enregistrement coché");
        public static string NoRecordCheckedMessage => L(
            "Check in step 2 the deleted records you want to restore.",
            "Cochez à l'étape 2 les enregistrements supprimés que vous voulez restaurer.");
        public static string RecoveryFinished => L("Recovery finished", "Restauration terminée");
        public static string RecoveryFinishedWithErrors => L("Recovery finished with errors", "Restauration terminée avec des erreurs");
        public static string RecoveryResult(int restored, int withWarnings, int skipped, int failed) => F(
            "{0} record(s) restored, {1} restored with warnings, {2} skipped, {3} failed.{4}{4}The \"Recovery log\" panel shows the result of each record.",
            "{0} enregistrement(s) restauré(s), {1} restauré(s) avec avertissements, {2} ignoré(s), {3} en échec.{4}{4}Le panneau « Journal de restauration » montre le résultat de chaque enregistrement.",
            restored, withWarnings, skipped, failed, Environment.NewLine);

        public static string RestoredRemoved(int restored, int remaining) => F(
            "{0} record(s) restored and removed from the list, {1} deleted record(s) left.",
            "{0} enregistrement(s) restauré(s) et retiré(s) de la liste, {1} enregistrement(s) supprimé(s) restant(s).", restored, remaining);
        public static string OpenRecordQuestion(string name) => F(
            "Do you want to open the restored record \"{0}\" in your browser?",
            "Voulez-vous ouvrir l'enregistrement restauré « {0} » dans votre navigateur ?", name);
        public static string OpenRecordsQuestion(int count) => F(
            "Do you want to open the {0} restored main records in your browser?",
            "Voulez-vous ouvrir les {0} enregistrements principaux restaurés dans votre navigateur ?", count);
        public static string OpenFirstRecordsQuestion(int count, int max) => F(
            "Do you want to open the first {1} of the {0} restored main records in your browser?",
            "Voulez-vous ouvrir dans votre navigateur les {1} premiers des {0} enregistrements principaux restaurés ?", count, max);
        public static string OpenRecord => L("Open the record", "Ouvrir l'enregistrement");
        public static string OpenRecords => L("Open the records", "Ouvrir les enregistrements");
        public static string NotNow => L("Not now", "Plus tard");

        public static string SomethingWentWrong => L("Something went wrong", "Une erreur est survenue");
        public static string MissingPrivileges => L("Missing privileges", "Privilèges manquants");
        public static string PrivilegeIntro => L(
            "Your user account does not have the privileges required for this operation in the connected environment.",
            "Votre compte utilisateur n'a pas les privilèges nécessaires à cette opération dans l'environnement connecté.");
        public static string MissingPrivilege(string name) => F("Missing privilege: {0}", "Privilège manquant : {0}", name);
        public static string PrivilegeSearch => L(
            "• Searching deleted records requires the privileges \"View Audit History\" and \"View Audit Summary\".",
            "• La recherche des enregistrements supprimés nécessite les privilèges « View Audit History » et « View Audit Summary ».");
        public static string PrivilegeRestore => L(
            "• Restoring records requires the Create privilege on each restored table.",
            "• La restauration nécessite le privilège Créer sur chaque table restaurée.");
        public static string PrivilegeAskAdmin => L(
            "Ask a system administrator to add them to one of your security roles, then try again.",
            "Demandez à un administrateur système de les ajouter à l'un de vos rôles de sécurité, puis réessayez.");

        public static string PleaseWait => L("Please wait…", "Veuillez patienter…");
        public static string ExpandDetails => L("Open in a large window", "Ouvrir dans une grande fenêtre");
        public static string DetailsViewerTitle => L("Technical details", "Détails techniques");
        public static string DetailsViewerSubtitle => L("Read, scroll and copy the full message", "Lisez, faites défiler et copiez le message complet");
        public static string WordWrap => L("Wrap lines", "Retour à la ligne");
        public static string NothingToRestoreTitle => L("Nothing to restore", "Rien à restaurer");
        public static string NothingToRestoreMessage => L(
            "The checked records and their related records already exist in the environment: nothing is missing.",
            "Les enregistrements cochés et leurs enregistrements liés existent déjà dans l'environnement : rien ne manque.");
        public static string ShowDetails => L("Show technical details", "Afficher les détails techniques");
        public static string HideDetails => L("Hide technical details", "Masquer les détails techniques");
        public static string CopyDetails => L("Copy details", "Copier les détails");
        public static string Copied => L("Copied", "Copié");

        #endregion

        #region Confirmation dialog

        public static string ConfirmTitle => L("Review and confirm the recovery", "Vérifiez et confirmez la restauration");
        public static string ConfirmSubtitle(string environment) => F(
            "Nothing is written in \"{0}\" until you approve",
            "Rien n'est écrit dans « {0} » tant que vous n'avez pas validé", environment);
        public static string TreeTitle => L("Records to restore", "Enregistrements à restaurer");
        public static string TreeSubtitle => L("Uncheck the records you do not want to restore", "Décochez les enregistrements que vous ne voulez pas restaurer");
        public static string ExpandAll => L("Expand all", "Tout déplier");
        public static string CollapseAll => L("Collapse all", "Tout replier");
        public static string RecordDetails => L("Record details", "Détails de l'enregistrement");
        public static string ColPreviousValue => L("PREVIOUS VALUE", "VALEUR PRÉCÉDENTE");
        public static string ColRestored => L("RESTORED", "RESTAURÉ");
        public static string ColNote => L("NOTE", "NOTE");
        public static string Yes => L("Yes", "Oui");
        public static string No => L("No", "Non");
        public static string OptionPreserveCreatedOn => L(
            "Keep the original Created On date (the restore date is used otherwise)",
            "Conserver la date de création d'origine (sinon la date de restauration est utilisée)");
        public static string OptionBypass => L(
            "Do not run custom plug-ins and flows while restoring (requires the \"Bypass custom plug-ins\" privilege)",
            "Ne pas exécuter les plug-ins et flux personnalisés pendant la restauration (nécessite le privilège « Bypass custom plug-ins »)");
        public static string OptionConfirm(string environment) => F(
            "I have reviewed the records above and I approve their recovery in the environment \"{0}\"",
            "J'ai vérifié les enregistrements ci-dessus et j'approuve leur restauration dans l'environnement « {0} »", environment);
        public static string Cancel => L("Cancel", "Annuler");
        public static string RestoreCount(int count) => F("Restore {0} record(s)", "Restaurer {0} enregistrement(s)", count);
        public static string WarningsTag(int count) => F("{0} warning(s)", "{0} avertissement(s)", count);
        public static string RecordSubtitle(Guid id, DateTime deletedOn, string user) => F(
            "{0}  ·  deleted on {1:yyyy-MM-dd HH:mm:ss} by {2}",
            "{0}  ·  supprimé le {1:yyyy-MM-dd HH:mm:ss} par {2}", id, deletedOn, user ?? L("unknown", "inconnu"));
        public static string SelectedRecordMessage => L(
            "Record you selected. Greyed columns are managed by the system and cannot be restored.",
            "Enregistrement que vous avez sélectionné. Les colonnes grisées sont gérées par le système et ne peuvent pas être restaurées.");
        public static string ConfirmSummary(int included, int selected, int parents, int related, int existing, int warnings) => F(
            "{0} missing record(s) will be restored with their original GUID, parents first: {2} missing parent(s), {1} selected, {3} related record(s) deleted with them.{6}{4} record(s) already exist and are not proposed.  {5} warning(s).",
            "{0} enregistrement(s) manquant(s) seront restaurés avec leur GUID d'origine, parents en premier : {2} parent(s) manquant(s), {1} sélectionné(s), {3} enregistrement(s) lié(s) supprimé(s) avec eux.{6}{4} enregistrement(s) existent déjà et ne sont pas proposés.  {5} avertissement(s).",
            included, selected, parents, related, existing, warnings, Environment.NewLine);

        #endregion

        #region Services

        public static string SearchingRelated(int level) => F("Searching related deleted records (level {0})...", "Recherche des enregistrements liés supprimés (niveau {0})...", level);
        public static string CheckingReferences(int level) => F("Checking referenced records (level {0})...", "Vérification des enregistrements référencés (niveau {0})...", level);
        public static string CheckingExisting => L("Checking which records already exist...", "Vérification des enregistrements déjà existants...");
        public static string PreparingPreview => L("Preparing the preview...", "Préparation de l'aperçu...");
        public static string SearchingReferencedDeletion(string table, Guid id) => F(
            "Searching the deletion of the referenced {0} record {1}...",
            "Recherche de la suppression de l'enregistrement référencé {0} {1}...", table, id);
        public static string RelationDeletedWithParent(string column) => F("deleted with its parent, column '{0}'", "supprimé avec son parent, colonne '{0}'", column);
        public static string RelationReferencedBy(string column) => F("referenced by column '{0}'", "référencé par la colonne '{0}'", column);
        public static string NoteInRecovery => L("Referenced record is part of this recovery", "L'enregistrement référencé fait partie de cette restauration");
        public static string NoteMissingReference => L(
            "Referenced record no longer exists and its deletion was not found in the audit: lookup left empty",
            "L'enregistrement référencé n'existe plus et sa suppression est introuvable dans l'audit : recherche laissée vide");
        public static string WarningMissingReference(string column, string reference) => F(
            "Column '{0}': the referenced record {1} no longer exists and cannot be restored, the lookup will be left empty.",
            "Colonne '{0}' : l'enregistrement référencé {1} n'existe plus et ne peut pas être restauré, la recherche sera laissée vide.", column, reference);
        public static string NoteSameGuid => L("Same GUID", "Même GUID");
        public static string NoteCreatedOnKept => L(
            "Kept when the option 'Keep the original Created On date' is checked",
            "Conservée si l'option « Conserver la date de création d'origine » est cochée");
        public static string NoteCreatedOnSystem => L("System-managed column, set to the restore date", "Colonne gérée par le système, égale à la date de restauration");
        public static string NoteStateAfterCreate => L("Applied right after the creation", "Appliqué juste après la création");
        public static string NoteColumnRemoved => L("Column no longer exists in the table", "La colonne n'existe plus dans la table");
        public static string NoteDerived => L("Derived column", "Colonne dérivée");
        public static string NoteThroughGuid => L("Set through the record GUID", "Défini par le GUID de l'enregistrement");
        public static string NoteOverriddenCreatedOn => L(
            "Managed by the option 'Keep the original Created On date'",
            "Géré par l'option « Conserver la date de création d'origine »");
        public static string NoteSystemColumn => L(
            "System-managed column, cannot be set when creating a record",
            "Colonne gérée par le système, impossible à définir à la création d'un enregistrement");
        public static string SkipExcluded => L("Excluded from the recovery", "Exclu de la restauration");
        public static string SkipAlreadyExists => L("A record with this GUID already exists", "Un enregistrement avec ce GUID existe déjà");
        public static string SkipParentNotRestored => L("Its parent record was not restored", "Son enregistrement parent n'a pas été restauré");
        public static string RestoringRecord(int index, int total, string table, string name) => F(
            "Restoring record {0} of {1}: {2} {3}",
            "Restauration de l'enregistrement {0} sur {1} : {2} {3}", index, total, table, name);
        public static string LookupNotRestored(string column) => F(
            "Column '{0}' left empty: the referenced record was not restored",
            "Colonne '{0}' laissée vide : l'enregistrement référencé n'a pas été restauré", column);
        public static string LookupNotSet(string column, string error) => F(
            "Column '{0}' could not be set: {1}",
            "La colonne '{0}' n'a pas pu être renseignée : {1}", column, error);
        public static string LookupUnknownTable(string column) => F(
            "Column '{0}' left empty: the table of the referenced record is unknown",
            "Colonne '{0}' laissée vide : la table de l'enregistrement référencé est inconnue", column);
        public static string LookupMissing(string column, string table, Guid id) => F(
            "Column '{0}' left empty: the referenced {1} record {2} does not exist",
            "Colonne '{0}' laissée vide : l'enregistrement {1} référencé {2} n'existe pas", column, table, id);
        public static string StatusNotRestored(string error) => F(
            "Record created but its status could not be set back: {0}",
            "Enregistrement créé mais son statut n'a pas pu être rétabli : {0}", error);
        public static string RelatedSearchTruncated(int max) => F(
            "More than {0} deletions happened around this deletion: some related records may be missing from the plan.",
            "Plus de {0} suppressions ont eu lieu autour de cette suppression : des enregistrements liés peuvent manquer dans le plan.", max);
        public static string OwnerReplaced(string owner, string error) => F(
            "The original owner {0} could not be kept ({1}): the record now belongs to you.",
            "Le propriétaire d'origine {0} n'a pas pu être conservé ({1}) : l'enregistrement vous appartient désormais.", owner, error);
        public static string ClosedBySystem => L("Restored", "Restauré");
        public static string SearchingUnlinked => L(
            "Searching records unlinked by the deletions...",
            "Recherche des enregistrements détachés par les suppressions...");
        public static string SearchingAssociations => L(
            "Reading the N:N associations from the audit history...",
            "Lecture des associations N:N dans l'historique d'audit...");
        public static string AssociationLabel(string relationship) => F("N:N association · {0}", "Association N:N · {0}", relationship);
        public static string NoteAssociation => L("Associated again after the restore", "Réassociée après la restauration");
        public static string NoteAssociationMissing => L(
            "The related record no longer exists: association not restored",
            "L'enregistrement associé n'existe plus : association non restaurée");
        public static string RelinkLabel(string table) => F("Linked record · {0}", "Enregistrement rattaché · {0}", table);
        public static string NoteRelink => L(
            "Not deleted but its lookup was emptied by the deletion: linked again to this record",
            "Non supprimé mais sa recherche a été vidée par la suppression : rattaché de nouveau à cet enregistrement");
        public static string AssociationFailed(string relationship, string target, string error) => F(
            "Association '{0}' with {1} not restored: {2}",
            "Association '{0}' avec {1} non restaurée : {2}", relationship, target, error);
        public static string AssociationTargetMissing(string relationship, string target) => F(
            "Association '{0}' with {1} not restored: the related record does not exist",
            "Association '{0}' avec {1} non restaurée : l'enregistrement associé n'existe pas", relationship, target);
        public static string RelinkFailed(string record, string column, string error) => F(
            "{0} could not be linked again through '{1}': {2}",
            "{0} n'a pas pu être rattaché par '{1}' : {2}", record, column, error);
        public static string LinksSummary(int associations, int relinks) => F(
            "{0} N:N association(s) and {1} unlinked record(s) will be linked again.",
            "{0} association(s) N:N et {1} enregistrement(s) détaché(s) seront rattachés.", associations, relinks);
        public static string NoAuditDetail => L("No audit detail returned", "Aucun détail d'audit retourné");
        public static string AuditWithoutValues => L("The audit entry does not contain the column values", "L'entrée d'audit ne contient pas les valeurs des colonnes");
        public static string RecordCount(int count) => F("{0} record(s)", "{0} enregistrement(s)", count);
        public static string Binary => L("(binary)", "(binaire)");

        #endregion
    }
}
