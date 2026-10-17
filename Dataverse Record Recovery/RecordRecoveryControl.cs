using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dataverse_Record_Recovery.Forms;
using Dataverse_Record_Recovery.Localization;
using Dataverse_Record_Recovery.Models;
using Dataverse_Record_Recovery.Services;
using Dataverse_Record_Recovery.UI;
using Dataverse_Record_Recovery.UI.Controls;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using XrmToolBox.Extensibility;
using MetadataCache = Dataverse_Record_Recovery.Services.MetadataCache;

namespace Dataverse_Record_Recovery
{
    public partial class RecordRecoveryControl : PluginControlBase
    {
        private Settings mySettings;
        private MetadataCache metadata;
        private AuditService auditService;
        private RecoveryService recoveryService;
        private List<TableItem> tables = new List<TableItem>();
        private bool isLoaded;
        private RecoveryPlan lastPlan;

        /// <summary>Summary of the last search, rebuilt when the language changes</summary>
        private Func<string> searchSummary = () => Strings.DefaultSummary;

        private const int MaxRecordsToOpen = 10;
        private readonly ThemeRenderer renderer = new ThemeRenderer();
        private readonly ToolStripDropDownButton tsbLanguage = new ToolStripDropDownButton();
        private readonly LoadingOverlay loading = new LoadingOverlay();
        private bool isBusy;

        /// <summary>Changes with each new connection: results computed for a previous environment are dropped</summary>
        private int connectionVersion;

        public RecordRecoveryControl()
        {
            InitializeComponent();

            tsHeader.Renderer = renderer;
            tslLogo.Image = Theme.CreateLogo(30);
            tsbClose.Image = LineIcons.ToBitmap(IconKind.Close, 16, Color.White);
            tsbReload.Image = LineIcons.ToBitmap(IconKind.Reload, 16, Color.White);
            tsbAuthor.Image = LineIcons.ToBitmap(IconKind.Contact, 16, Color.White);
            tsbDonate.Image = LineIcons.ToBitmap(IconKind.Heart, 16, Color.FromArgb(255, 107, 129));

            Theme.StyleGrid(dgvRecords, true);
            Theme.StyleGrid(dgvDetails, true);
            Theme.StyleGrid(dgvLog, true);
            colDetailLogicalName.DefaultCellStyle.Font = Theme.Mono;
            colRecordId.DefaultCellStyle.Font = Theme.Mono;

            CreateLanguageMenu();
            ApplyLanguage();
            tsHeader.SizeChanged += (s, e) => FitHeader();

            Controls.Add(loading);
            dgvLog.CellDoubleClick += dgvLog_CellDoubleClick;
        }

        /// <summary>
        /// Runs a long operation in the background behind the loading layer. The work reports its steps
        /// through the given callback; the completion receives the error (if any) and the result.
        /// </summary>
        private void RunAsync(string message, Func<Action<string>, object> work, Action<Exception, object> completed, bool keepAfterConnectionChange = false)
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;

            loading.Begin(this, Strings.PleaseWait, message);
            pnlCanvas.Enabled = false;
            tsHeader.Enabled = false;

            var version = connectionVersion;
            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += (s, e) => e.Result = work(step => worker.ReportProgress(0, step));
            worker.ProgressChanged += (s, e) => loading.SetMessage(e.UserState as string);
            worker.RunWorkerCompleted += (s, e) =>
            {
                worker.Dispose();
                isBusy = false;
                if (IsDisposed)
                {
                    return;
                }
                pnlCanvas.Enabled = true;
                tsHeader.Enabled = true;
                loading.End();

                // Data read in the previous environment must never be shown, nor restored, in the new one
                if (version == connectionVersion)
                {
                    completed(e.Error, e.Error == null ? e.Result : null);
                    return;
                }
                if (keepAfterConnectionChange)
                {
                    completed(e.Error, e.Error == null ? e.Result : null);
                }
                if (Service != null)
                {
                    LoadTablesAndUsers();
                }
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Hides the subtitle of the header when the buttons need its room, instead of pushing them into the overflow menu
        /// </summary>
        private void FitHeader()
        {
            var used = tsHeader.Padding.Horizontal + 24;
            foreach (ToolStripItem item in tsHeader.Items)
            {
                if (item != tslSubtitle && item.Available)
                {
                    used += item.GetPreferredSize(Size.Empty).Width + item.Margin.Horizontal;
                }
            }
            tslSubtitle.Visible = used + tslSubtitle.GetPreferredSize(Size.Empty).Width + tslSubtitle.Margin.Horizontal <= tsHeader.Width;
        }

        #region Language

        private void CreateLanguageMenu()
        {
            tsbLanguage.Alignment = ToolStripItemAlignment.Right;
            tsbLanguage.Padding = new Padding(6, 0, 6, 0);
            tsbLanguage.Image = LineIcons.ToBitmap(IconKind.Globe, 16, Color.White);
            tsbLanguage.DropDown.Renderer = renderer;
            foreach (AppLanguage language in Enum.GetValues(typeof(AppLanguage)))
            {
                var item = new ToolStripMenuItem(Strings.NativeName(language)) { Tag = language };
                item.Click += (s, e) => SetLanguage((AppLanguage)((ToolStripItem)s).Tag);
                tsbLanguage.DropDownItems.Add(item);
            }

            // Right-aligned items are laid out from the right: the language sits between "Reload tables" and the links
            tsHeader.Items.Insert(tsHeader.Items.IndexOf(tsbReload), tsbLanguage);
        }

        private void SetLanguage(AppLanguage language)
        {
            Strings.Current = language;
            if (mySettings != null)
            {
                mySettings.Language = Strings.Code;
                SettingsManager.Instance.Save(GetType(), mySettings);
            }
            ApplyLanguage();
        }

        /// <summary>
        /// Writes every text of the screen in the current language
        /// </summary>
        private void ApplyLanguage()
        {
            tsbLanguage.Text = Strings.NativeName(Strings.Current);
            tsbLanguage.ToolTipText = Strings.LanguageTip;
            foreach (ToolStripMenuItem item in tsbLanguage.DropDownItems)
            {
                item.Checked = (AppLanguage)item.Tag == Strings.Current;
            }

            tslSubtitle.Text = Strings.HeaderSubtitle;
            tsbClose.Text = Strings.Close;
            tsbAuthor.ToolTipText = Strings.AuthorTip;
            tsbDonate.Text = Strings.Donate;
            tsbDonate.ToolTipText = Strings.DonateTip;
            tsbReload.Text = Strings.ReloadTables;
            tsbReload.ToolTipText = Strings.ReloadTablesTip;
            UpdateConnectionInfo();
            FitHeader();

            cardFilters.Title = Strings.FiltersTitle;
            cardFilters.Subtitle = Strings.FiltersSubtitle;
            lblTable.Text = Strings.Table;
            lblUser.Text = Strings.DeletedBy;
            lblFrom.Text = Strings.DeletedFrom;
            lblTo.Text = Strings.To;
            lblMaxRecords.Text = Strings.MaxRecords;
            btnSearch.Text = Strings.Search;
            chkAuditedOnly.Text = Strings.AuditedOnly;
            chkRelated.Text = Strings.IncludeRelated;
            lblWindow.Text = Strings.WindowSuffix;
            toolTip.SetToolTip(chkAuditedOnly, Strings.AuditedOnlyTip);
            toolTip.SetToolTip(nudWindow, Strings.WindowTip);
            toolTip.SetToolTip(nudMaxRecords, Strings.MaxRecordsTip);

            // The window field follows the checkbox, whose text length depends on the language
            nudWindow.Left = chkRelated.Right + 4;
            lblWindow.Left = nudWindow.Right + 6;

            cardRecords.Title = Strings.RecordsTitle;
            cardRecords.Subtitle = Strings.RecordsSubtitle;
            btnCheckAll.Text = Strings.CheckAll;
            btnUncheckAll.Text = Strings.UncheckAll;
            colName.HeaderText = Strings.ColDeletedRecord;
            colRecordId.HeaderText = Strings.ColGuid;
            colDeletedOn.HeaderText = Strings.ColDeletedOn;
            colDeletedBy.HeaderText = Strings.ColDeletedBy;

            cardDetails.Title = Strings.DetailsTitle;
            cardDetails.Subtitle = Strings.DetailsSubtitle;
            colDetailColumn.HeaderText = Strings.ColColumn;
            colDetailLogicalName.HeaderText = Strings.ColLogicalName;
            colDetailValue.HeaderText = Strings.ColValue;

            cardLog.Title = Strings.LogTitle;
            cardLog.Subtitle = Strings.LogSubtitle;
            colLogResult.HeaderText = Strings.ColResult;
            colLogRole.HeaderText = Strings.ColRole;
            colLogTable.HeaderText = Strings.ColTable;
            colLogName.HeaderText = Strings.ColRecord;
            colLogMessage.HeaderText = Strings.ColMessage;

            // Content already shown
            if (cmbUser.Items.Count > 0)
            {
                var index = cmbUser.SelectedIndex;
                cmbUser.Items[0] = UserItem.All;
                cmbUser.SelectedIndex = index;
            }
            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                var record = (DeletedRecord)row.Tag;
                row.Cells[1].Value = string.IsNullOrEmpty(record.Name) ? Strings.NoName : record.Name;
                RefreshStatus(row);
            }
            if (lastPlan != null)
            {
                ShowLog(lastPlan);
            }

            RefreshSummary();
        }

        #endregion Language

        private void RecordRecoveryControl_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            Strings.Current = Strings.FromCode(mySettings.Language);
            ApplyLanguage();

            chkAuditedOnly.Checked = mySettings.AuditedTablesOnly;
            nudMaxRecords.Value = Math.Max(nudMaxRecords.Minimum, Math.Min(nudMaxRecords.Maximum, mySettings.MaxRecords));
            chkRelated.Checked = mySettings.IncludeRelated;
            nudWindow.Value = Math.Max(nudWindow.Minimum, Math.Min(nudWindow.Maximum, mySettings.RelatedWindowSeconds));
            dtpFrom.Value = DateTime.Now.AddDays(-7);
            dtpTo.Value = DateTime.Now;

            isLoaded = true;
            UpdateConnectionInfo();
            if (Service != null)
            {
                LoadTablesAndUsers();
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbReload_Click(object sender, EventArgs e)
        {
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            ExecuteMethod(LoadTablesAndUsers);
        }

        private void tsbAuthor_Click(object sender, EventArgs e)
        {
            OpenUrl(RecordRecoveryPlugin.LinkedInUrl);
        }

        private void tsbDonate_Click(object sender, EventArgs e)
        {
            OpenUrl(RecordRecoveryPlugin.DonateUrl);
        }

        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                LogError("Unable to open {0}: {1}", url, ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            ExecuteMethod(SearchDeletedRecords);
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            ExecuteMethod(PreviewRecovery);
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(true);
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(false);
        }

        private void UpdateConnectionInfo()
        {
            var name = ConnectionDetail?.ConnectionName ?? ConnectionDetail?.OrganizationFriendlyName;
            tslConnection.Text = string.IsNullOrEmpty(name) ? Strings.NotConnected : Strings.ConnectedTo(name);
            FitHeader();
        }

        #region Tables and users

        private void LoadTablesAndUsers()
        {
            ClearResults();

            RunAsync(Strings.LoadingTables,
                progress =>
                {
                    var cache = new MetadataCache(Service);
                    cache.LoadEntities();

                    progress(Strings.LoadingUsers);
                    var users = new List<UserItem>();
                    var query = new QueryExpression("systemuser")
                    {
                        ColumnSet = new ColumnSet("fullname", "domainname", "isdisabled"),
                        NoLock = true,
                        PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
                    };
                    query.AddOrder("fullname", OrderType.Ascending);
                    while (true)
                    {
                        var page = Service.RetrieveMultiple(query);
                        users.AddRange(page.Entities.Select(u => new UserItem(u)));
                        if (!page.MoreRecords)
                        {
                            break;
                        }
                        query.PageInfo.PageNumber++;
                        query.PageInfo.PagingCookie = page.PagingCookie;
                    }

                    var organization = Service.RetrieveMultiple(new QueryExpression("organization")
                    {
                        ColumnSet = new ColumnSet("isauditenabled")
                    }).Entities.FirstOrDefault();

                    return Tuple.Create(cache, users, organization?.GetAttributeValue<bool?>("isauditenabled") ?? true);
                },
                (error, value) =>
                {
                    if (error != null)
                    {
                        MessageDialog.ShowError(this, error);
                        return;
                    }

                    var result = (Tuple<MetadataCache, List<UserItem>, bool>)value;
                    metadata = result.Item1;
                    auditService = new AuditService(Service, metadata);
                    recoveryService = new RecoveryService(Service, metadata, auditService);

                    tables = metadata.Entities
                        .Where(m => m.ObjectTypeCode != null && m.IsIntersect != true)
                        .Select(m => new TableItem(m))
                        .OrderBy(t => t.ToString(), StringComparer.CurrentCultureIgnoreCase)
                        .ToList();
                    FillTables();

                    cmbUser.Items.Clear();
                    cmbUser.Items.Add(UserItem.All);
                    cmbUser.Items.AddRange(result.Item2.Cast<object>().ToArray());
                    cmbUser.SelectedIndex = 0;

                    if (result.Item3)
                    {
                        HideNotification();
                    }
                    else
                    {
                        ShowWarningNotification(Strings.AuditDisabled, null);
                    }
                });
        }

        private void FillTables()
        {
            var current = (cmbTable.SelectedItem as TableItem)?.Metadata.LogicalName ?? mySettings?.LastTable;

            cmbTable.BeginUpdate();
            cmbTable.Items.Clear();
            cmbTable.Items.AddRange(tables.Where(t => !chkAuditedOnly.Checked || MetadataCache.IsAudited(t.Metadata)).Cast<object>().ToArray());
            cmbTable.EndUpdate();

            cmbTable.SelectedItem = cmbTable.Items.Cast<TableItem>().FirstOrDefault(t => t.Metadata.LogicalName == current);
        }

        private void chkAuditedOnly_CheckedChanged(object sender, EventArgs e)
        {
            FillTables();
        }

        private void chkRelated_CheckedChanged(object sender, EventArgs e)
        {
            nudWindow.Enabled = chkRelated.Checked;
        }

        #endregion Tables and users

        #region Search

        private void SearchDeletedRecords()
        {
            if (metadata == null)
            {
                LoadTablesAndUsers();
                return;
            }

            var table = cmbTable.SelectedItem as TableItem
                ?? tables.FirstOrDefault(t => string.Equals(t.ToString(), cmbTable.Text.Trim(), StringComparison.CurrentCultureIgnoreCase));
            if (table == null)
            {
                MessageDialog.Show(this, MessageKind.Info, Strings.SelectTableTitle, Strings.SelectTableMessage);
                return;
            }

            var user = cmbUser.SelectedItem as UserItem
                ?? cmbUser.Items.Cast<UserItem>().FirstOrDefault(u => string.Equals(u.ToString(), cmbUser.Text.Trim(), StringComparison.CurrentCultureIgnoreCase));
            if (user == null && cmbUser.Text.Trim().Length > 0)
            {
                MessageDialog.Show(this, MessageKind.Info, Strings.UnknownUserTitle, Strings.UnknownUserMessage);
                return;
            }

            if (dtpFrom.Value >= dtpTo.Value)
            {
                MessageDialog.Show(this, MessageKind.Info, Strings.DateRangeTitle, Strings.DateRangeMessage);
                return;
            }

            var logicalName = table.Metadata.LogicalName;
            var userId = user?.Id;
            var fromUtc = dtpFrom.Value.ToUniversalTime();
            var toUtc = dtpTo.Value.ToUniversalTime();
            var maxRecords = (int)nudMaxRecords.Value;

            mySettings.LastTable = logicalName;
            ClearResults();

            RunAsync(Strings.SearchingDeleted,
                progress =>
                {
                    bool truncated;
                    var records = auditService.Search(new[] { logicalName }, userId, fromUtc, toUtc, maxRecords, out truncated);

                    // A record restored since its deletion exists again: there is nothing to recover
                    var existing = recoveryService.FindExisting(logicalName, records.Select(r => r.RecordId).Distinct().ToList());
                    var hidden = existing == null ? 0 : records.RemoveAll(r => existing.Contains(r.RecordId));

                    metadata.GetFull(logicalName);
                    auditService.LoadDetails(records, (done, total) => progress(Strings.LoadingDetails(done, total)));

                    return Tuple.Create(records, truncated, hidden);
                },
                (error, value) =>
                {
                    if (error != null)
                    {
                        MessageDialog.ShowError(this, error);
                        return;
                    }

                    var result = (Tuple<List<DeletedRecord>, bool, int>)value;
                    var count = result.Item1.Count;
                    var truncated = result.Item2;
                    var hidden = result.Item3;
                    var tableName = table.ToString();
                    searchSummary = () => (count == 0
                        ? Strings.NoRecordFound(tableName)
                        : truncated
                            ? Strings.SearchTruncated(count)
                            : Strings.RecordsFound(count, tableName))
                        + (hidden > 0 ? " " + Strings.ExistingHidden(hidden) : string.Empty);
                    ShowRecords(result.Item1);
                });
        }

        private void ShowRecords(List<DeletedRecord> records)
        {
            dgvRecords.Rows.Clear();
            foreach (var record in records)
            {
                var index = dgvRecords.Rows.Add(
                    false,
                    string.IsNullOrEmpty(record.Name) ? Strings.NoName : record.Name,
                    record.RecordId.ToString(),
                    record.DeletedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    record.DeletedBy?.Name ?? string.Empty);
                dgvRecords.Rows[index].Tag = record;
                RefreshStatus(dgvRecords.Rows[index]);
            }
            RefreshSummary();

            // The first row was selected while it was being added, before its record was attached
            dgvRecords_SelectionChanged(dgvRecords, EventArgs.Empty);
        }

        private static void RefreshStatus(DataGridViewRow row)
        {
            var record = (DeletedRecord)row.Tag;

            // Records without audit details cannot be picked: they are greyed and the reason is in the tooltip
            row.Cells[0].ReadOnly = record.OldValue == null;
            var reason = record.OldValue == null ? Strings.StatusCannotRestore(record.Error) : string.Empty;
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.ToolTipText = reason;
            }
            if (record.OldValue == null)
            {
                row.DefaultCellStyle.ForeColor = Theme.Subtle;
                row.DefaultCellStyle.SelectionForeColor = Theme.Subtle;
            }
        }

        private List<DeletedRecord> GetCheckedRecords()
        {
            return dgvRecords.Rows.Cast<DataGridViewRow>()
                .Where(r => Equals(r.Cells[0].Value, true))
                .Select(r => (DeletedRecord)r.Tag)
                .ToList();
        }

        private void RefreshSummary()
        {
            var count = GetCheckedRecords().Count;
            var summary = searchSummary();
            lblSummary.Text = count == 0 ? summary : Strings.SummaryChecked(summary, count);
            btnRestore.Text = count == 0 ? Strings.PreviewAndRestore : Strings.PreviewAndRestoreCount(count);
            btnRestore.Enabled = count > 0;
            btnCheckAll.Enabled = btnUncheckAll.Enabled = dgvRecords.Rows.Count > 0;
            LayoutRecordActions();
        }

        /// <summary>
        /// Sizes the buttons of step 2 to their text, which depends on the language and on the number of checked records
        /// </summary>
        private void LayoutRecordActions()
        {
            btnCheckAll.Width = btnCheckAll.GetPreferredSize(Size.Empty).Width;
            btnUncheckAll.Left = btnCheckAll.Right + 4;
            btnUncheckAll.Width = btnUncheckAll.GetPreferredSize(Size.Empty).Width;

            var restoreWidth = Math.Max(270, btnRestore.GetPreferredSize(Size.Empty).Width);
            btnRestore.SetBounds(pnlRecordActions.ClientSize.Width - restoreWidth, btnRestore.Top, restoreWidth, btnRestore.Height);

            var summaryLeft = btnUncheckAll.Right + 12;
            lblSummary.SetBounds(summaryLeft, lblSummary.Top, Math.Max(0, btnRestore.Left - 12 - summaryLeft), lblSummary.Height);
        }

        private void ClearResults()
        {
            dgvRecords.Rows.Clear();
            dgvDetails.Rows.Clear();
            dgvLog.Rows.Clear();
            lastPlan = null;
            searchSummary = () => Strings.DefaultSummary;
            RefreshSummary();
        }

        private void SetAllChecked(bool isChecked)
        {
            dgvRecords.EndEdit();
            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                if (!row.Cells[0].ReadOnly)
                {
                    row.Cells[0].Value = isChecked;
                }
            }
            RefreshSummary();
        }

        private void dgvRecords_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // A checkbox click is applied immediately instead of when the cell loses the focus
            if (dgvRecords.IsCurrentCellDirty)
            {
                dgvRecords.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvRecords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                RefreshSummary();
            }
        }

        private void dgvRecords_SelectionChanged(object sender, EventArgs e)
        {
            dgvDetails.Rows.Clear();

            var record = dgvRecords.CurrentRow?.Tag as DeletedRecord;
            if (record?.OldValue == null)
            {
                return;
            }

            var attributes = metadata?.TryGetAttributes(record.EntityLogicalName);
            var rows = new List<string[]>();
            foreach (var pair in record.OldValue.Attributes)
            {
                AttributeMetadata attribute = null;
                attributes?.TryGetValue(pair.Key, out attribute);
                if (attribute?.AttributeOf != null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    attribute == null ? pair.Key : MetadataCache.GetDisplayName(attribute),
                    pair.Key,
                    ValueFormatter.Format(pair.Value, attribute)
                });
            }

            foreach (var row in rows.OrderBy(r => r[0], StringComparer.CurrentCultureIgnoreCase))
            {
                dgvDetails.Rows.Add(row[0], row[1], row[2]);
            }
            dgvDetails.ClearSelection();
        }

        #endregion Search

        #region Recovery

        private void PreviewRecovery()
        {
            dgvRecords.EndEdit();
            var selected = GetCheckedRecords();
            if (selected.Count == 0)
            {
                MessageDialog.Show(this, MessageKind.Info, Strings.NoRecordCheckedTitle, Strings.NoRecordCheckedMessage);
                return;
            }

            var includeRelated = chkRelated.Checked;
            var window = (int)nudWindow.Value;

            RunAsync(Strings.Analyzing,
                progress =>
                {
                    return recoveryService.BuildPlan(selected, includeRelated, window, progress);
                },
                (error, value) =>
                {
                    if (error != null)
                    {
                        MessageDialog.ShowError(this, error);
                        return;
                    }

                    var plan = (RecoveryPlan)value;
                    if (plan.Items.Count == 0)
                    {
                        MessageDialog.Show(this, MessageKind.Info, Strings.NothingToRestoreTitle, Strings.NothingToRestoreMessage);
                        return;
                    }
                    var environment = ConnectionDetail?.OrganizationFriendlyName ?? ConnectionDetail?.ConnectionName ?? Strings.CurrentEnvironment;

                    // Nothing is written before the user approves the content of the plan
                    using (var dialog = new RecoveryConfirmationDialog(plan, metadata, environment, mySettings.PreserveCreatedOn))
                    {
                        if (dialog.ShowModal(this) != DialogResult.OK)
                        {
                            return;
                        }

                        var options = dialog.Options;
                        mySettings.PreserveCreatedOn = options.PreserveCreatedOn;
                        Restore(plan, options);
                    }
                });
        }

        private void Restore(RecoveryPlan plan, RecoveryOptions options)
        {
            // Links to the restored records point to the environment where they were written, even if the connection changes meanwhile
            var baseUrl = ConnectionDetail?.WebApplicationUrl;

            RunAsync(Strings.Restoring,
                progress =>
                {
                    recoveryService.Restore(plan, options, progress);
                    return null;
                },
                (error, value) =>
                {
                    lastPlan = plan;
                    ShowLog(plan);
                    RemoveRestoredRecords(plan);

                    if (error != null)
                    {
                        MessageDialog.ShowError(this, error);
                        return;
                    }

                    var restored = plan.Items.Count(i => i.Status == RecoveryStatus.Restored);
                    var withWarnings = plan.Items.Count(i => i.Status == RecoveryStatus.RestoredWithWarnings);
                    var skipped = plan.Items.Count(i => i.Status == RecoveryStatus.Skipped);
                    var failed = plan.Items.Count(i => i.Status == RecoveryStatus.Failed);

                    // Failures are per record: the first one is shown so that the cause is visible without opening the log
                    var firstFailure = plan.OrderedItems.FirstOrDefault(i => i.Status == RecoveryStatus.Failed);
                    var kind = failed > 0 ? MessageKind.Error : withWarnings > 0 ? MessageKind.Warning : MessageKind.Success;
                    var title = failed > 0 ? Strings.RecoveryFinishedWithErrors : Strings.RecoveryFinished;
                    var message = Strings.RecoveryResult(restored, withWarnings, skipped, failed);
                    var details = firstFailure == null ? null : $"{firstFailure.Record.EntityLogicalName} {firstFailure.Record.Name} ({firstFailure.Record.RecordId}):{Environment.NewLine}{firstFailure.Message}";

                    // The records picked by the user can be opened right away to check them
                    var mainRecords = plan.Roots.Where(IsRestored).Select(i => i.Record).ToList();
                    if (mainRecords.Count == 0 || string.IsNullOrEmpty(baseUrl))
                    {
                        MessageDialog.Show(this, kind, title, message, details);
                        return;
                    }

                    var question = mainRecords.Count == 1
                        ? Strings.OpenRecordQuestion(string.IsNullOrEmpty(mainRecords[0].Name) ? mainRecords[0].RecordId.ToString() : mainRecords[0].Name)
                        : mainRecords.Count <= MaxRecordsToOpen
                            ? Strings.OpenRecordsQuestion(mainRecords.Count)
                            : Strings.OpenFirstRecordsQuestion(mainRecords.Count, MaxRecordsToOpen);
                    var open = MessageDialog.Ask(this, kind, title, message, details, question,
                        mainRecords.Count == 1 ? Strings.OpenRecord : Strings.OpenRecords, Strings.NotNow);
                    if (open)
                    {
                        foreach (var record in mainRecords.Take(MaxRecordsToOpen))
                        {
                            OpenUrl(GetRecordUrl(baseUrl, record));
                        }
                    }
                },
                keepAfterConnectionChange: true);
        }

        private static bool IsRestored(RecoveryItem item)
        {
            return item.Status == RecoveryStatus.Restored || item.Status == RecoveryStatus.RestoredWithWarnings;
        }

        private static string GetRecordUrl(string webApplicationUrl, DeletedRecord record)
        {
            return $"{webApplicationUrl.TrimEnd('/')}/main.aspx?pagetype=entityrecord&etn={Uri.EscapeDataString(record.EntityLogicalName)}&id={record.RecordId}";
        }

        /// <summary>
        /// Restored records are no longer deleted: they leave the list, whether they were picked or restored with another record
        /// </summary>
        private void RemoveRestoredRecords(RecoveryPlan plan)
        {
            var restoredIds = new HashSet<Guid>(plan.Items.Where(IsRestored).Select(i => i.Record.RecordId));
            var removed = dgvRecords.Rows.Cast<DataGridViewRow>()
                .Where(r => restoredIds.Contains(((DeletedRecord)r.Tag).RecordId))
                .ToList();
            if (removed.Count == 0)
            {
                RefreshSummary();
                return;
            }

            foreach (var row in removed)
            {
                dgvRecords.Rows.Remove(row);
            }

            var restored = removed.Count;
            var remaining = dgvRecords.Rows.Count;
            searchSummary = () => Strings.RestoredRemoved(restored, remaining);
            RefreshSummary();
            dgvRecords_SelectionChanged(dgvRecords, EventArgs.Empty);
        }

        private void ShowLog(RecoveryPlan plan)
        {
            dgvLog.Rows.Clear();
            foreach (var item in plan.OrderedItems)
            {
                var index = dgvLog.Rows.Add(
                    GetStatusLabel(item.Status),
                    item.Role == RecoveryRole.Selected ? Strings.RoleSelected : item.Role == RecoveryRole.MissingParent ? Strings.RoleMissingParent : Strings.RoleRelated,
                    metadata?.GetDisplayName(item.Record.EntityLogicalName) ?? item.Record.EntityLogicalName,
                    string.IsNullOrEmpty(item.Record.Name) ? item.Record.RecordId.ToString() : item.Record.Name,
                    item.Message);
                var row = dgvLog.Rows[index];
                row.Tag = item;
                row.Cells[3].ToolTipText = item.Record.RecordId.ToString();

                Color color;
                switch (item.Status)
                {
                    case RecoveryStatus.Restored:
                        color = Theme.SuccessText;
                        break;

                    case RecoveryStatus.RestoredWithWarnings:
                        color = Theme.WarningText;
                        break;

                    case RecoveryStatus.Failed:
                        color = Theme.Danger;
                        break;

                    default:
                        color = Theme.Subtle;
                        break;
                }
                row.Cells[0].Style.ForeColor = color;
                row.Cells[0].Style.SelectionForeColor = color;
                row.Cells[0].Style.Font = Theme.Semibold;
            }
            dgvLog.ClearSelection();
        }

        private void dgvLog_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var item = e.RowIndex >= 0 ? dgvLog.Rows[e.RowIndex].Tag as RecoveryItem : null;
            if (item == null)
            {
                return;
            }

            var record = item.Record;
            var name = string.IsNullOrEmpty(record.Name) ? record.RecordId.ToString() : record.Name;
            var text = $"{metadata?.GetDisplayName(record.EntityLogicalName) ?? record.EntityLogicalName} ({record.EntityLogicalName})  ·  {name}{Environment.NewLine}"
                + $"GUID: {record.RecordId}{Environment.NewLine}"
                + $"{Strings.ColResult}: {GetStatusLabel(item.Status)}{Environment.NewLine}{Environment.NewLine}"
                + string.Join(Environment.NewLine + Environment.NewLine, item.Messages);
            TextViewerDialog.Show(this, name, GetStatusLabel(item.Status), text);
        }

        private static string GetStatusLabel(RecoveryStatus status)
        {
            switch (status)
            {
                case RecoveryStatus.Restored:
                    return Strings.ResultRestored;

                case RecoveryStatus.RestoredWithWarnings:
                    return Strings.ResultRestoredWithWarnings;

                case RecoveryStatus.Skipped:
                    return Strings.ResultSkipped;

                case RecoveryStatus.Failed:
                    return Strings.ResultFailed;

                default:
                    return Strings.ResultNotProcessed;
            }
        }

        #endregion Recovery

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordRecoveryControl_OnCloseTool(object sender, EventArgs e)
        {
            if (mySettings == null)
            {
                return;
            }

            mySettings.AuditedTablesOnly = chkAuditedOnly.Checked;
            mySettings.MaxRecords = (int)nudMaxRecords.Value;
            mySettings.IncludeRelated = chkRelated.Checked;
            mySettings.RelatedWindowSeconds = (int)nudWindow.Value;

            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            // Data of the previous environment must never be restored in the new one
            connectionVersion++;
            metadata = null;
            auditService = null;
            recoveryService = null;

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }

            UpdateConnectionInfo();

            if (isLoaded && newService != null)
            {
                LoadTablesAndUsers();
            }
        }

        private class TableItem
        {
            private readonly string text;

            public TableItem(EntityMetadata metadata)
            {
                Metadata = metadata;
                text = $"{MetadataCache.GetDisplayName(metadata)} ({metadata.LogicalName})";
            }

            public EntityMetadata Metadata { get; }

            public override string ToString()
            {
                return text;
            }
        }

        private class UserItem
        {
            public static readonly UserItem All = new UserItem();

            private readonly string text;
            private readonly bool isDisabled;

            private UserItem()
            {
            }

            public UserItem(Entity user)
            {
                Id = user.Id;

                var name = user.GetAttributeValue<string>("fullname");
                var domainName = user.GetAttributeValue<string>("domainname");
                text = string.IsNullOrEmpty(domainName) ? name : $"{name} ({domainName})";
                isDisabled = user.GetAttributeValue<bool>("isdisabled");
            }

            public Guid? Id { get; }

            public override string ToString()
            {
                if (!Id.HasValue)
                {
                    return Strings.AllUsers;
                }
                return isDisabled ? $"{text} [{Strings.Disabled}]" : text ?? string.Empty;
            }
        }
    }
}
