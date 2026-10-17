namespace Dataverse_Record_Recovery
{
    partial class RecordRecoveryControl
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tsHeader = new System.Windows.Forms.ToolStrip();
            this.tslLogo = new System.Windows.Forms.ToolStripLabel();
            this.tslSubtitle = new System.Windows.Forms.ToolStripLabel();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssHeaderSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAuthor = new System.Windows.Forms.ToolStripButton();
            this.tsbDonate = new System.Windows.Forms.ToolStripButton();
            this.tssLinksSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsbReload = new System.Windows.Forms.ToolStripButton();
            this.tslConnection = new System.Windows.Forms.ToolStripLabel();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.cardRecords = new Dataverse_Record_Recovery.UI.Controls.CardPanel();
            this.dgvRecords = new System.Windows.Forms.DataGridView();
            this.colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecordId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeletedOn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeletedBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlRecordActions = new System.Windows.Forms.Panel();
            this.btnCheckAll = new Dataverse_Record_Recovery.UI.Controls.ModernButton();
            this.btnUncheckAll = new Dataverse_Record_Recovery.UI.Controls.ModernButton();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnRestore = new Dataverse_Record_Recovery.UI.Controls.ModernButton();
            this.splitBottom = new System.Windows.Forms.SplitContainer();
            this.cardDetails = new Dataverse_Record_Recovery.UI.Controls.CardPanel();
            this.dgvDetails = new System.Windows.Forms.DataGridView();
            this.colDetailColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDetailLogicalName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDetailValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cardLog = new Dataverse_Record_Recovery.UI.Controls.CardPanel();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.colLogResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogRole = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFilterSpacer = new System.Windows.Forms.Panel();
            this.cardFilters = new Dataverse_Record_Recovery.UI.Controls.CardPanel();
            this.lblTable = new System.Windows.Forms.Label();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.cmbUser = new System.Windows.Forms.ComboBox();
            this.lblFrom = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblMaxRecords = new System.Windows.Forms.Label();
            this.nudMaxRecords = new System.Windows.Forms.NumericUpDown();
            this.btnSearch = new Dataverse_Record_Recovery.UI.Controls.ModernButton();
            this.chkAuditedOnly = new System.Windows.Forms.CheckBox();
            this.chkRelated = new System.Windows.Forms.CheckBox();
            this.nudWindow = new System.Windows.Forms.NumericUpDown();
            this.lblWindow = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tsHeader.SuspendLayout();
            this.pnlCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.cardRecords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecords)).BeginInit();
            this.pnlRecordActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitBottom)).BeginInit();
            this.splitBottom.Panel1.SuspendLayout();
            this.splitBottom.Panel2.SuspendLayout();
            this.splitBottom.SuspendLayout();
            this.cardDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).BeginInit();
            this.cardLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            this.cardFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxRecords)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWindow)).BeginInit();
            this.SuspendLayout();
            //
            // tsHeader
            //
            this.tsHeader.AutoSize = false;
            this.tsHeader.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsHeader.ImageScalingSize = new System.Drawing.Size(16, 16);
            this.tsHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslLogo,
            this.tslSubtitle,
            this.tsbClose,
            this.tssHeaderSeparator,
            this.tsbAuthor,
            this.tsbDonate,
            this.tssLinksSeparator,
            this.tsbReload,
            this.tslConnection});
            this.tsHeader.Location = new System.Drawing.Point(0, 0);
            this.tsHeader.Name = "tsHeader";
            this.tsHeader.Padding = new System.Windows.Forms.Padding(10, 0, 10, 3);
            this.tsHeader.Size = new System.Drawing.Size(1280, 60);
            this.tsHeader.TabIndex = 0;
            this.tsHeader.Tag = "header";
            //
            // tslLogo
            //
            this.tslLogo.Font = new System.Drawing.Font("Segoe UI Semibold", 13F);
            this.tslLogo.ForeColor = System.Drawing.Color.White;
            this.tslLogo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tslLogo.Margin = new System.Windows.Forms.Padding(4, 1, 6, 2);
            this.tslLogo.Name = "tslLogo";
            this.tslLogo.Text = "Record Recovery";
            //
            // tslSubtitle
            //
            this.tslSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(201)))), ((int)(((byte)(199)))), ((int)(((byte)(242)))));
            this.tslSubtitle.Margin = new System.Windows.Forms.Padding(6, 4, 0, 2);
            this.tslSubtitle.Name = "tslSubtitle";
            this.tslSubtitle.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.tslSubtitle.Text = "Restore deleted Dataverse records from the audit history, with their original GUID";
            //
            // tsbClose
            //
            this.tsbClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbClose.Margin = new System.Windows.Forms.Padding(2, 1, 0, 2);
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            //
            // tssHeaderSeparator
            //
            this.tssHeaderSeparator.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tssHeaderSeparator.Name = "tssHeaderSeparator";
            //
            // tsbAuthor
            //
            this.tsbAuthor.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbAuthor.Name = "tsbAuthor";
            this.tsbAuthor.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tsbAuthor.Text = "Hamza AMRAOUI";
            this.tsbAuthor.ToolTipText = "Developed by Hamza AMRAOUI (LinkedIn)";
            this.tsbAuthor.Click += new System.EventHandler(this.tsbAuthor_Click);
            //
            // tsbDonate
            //
            this.tsbDonate.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbDonate.Name = "tsbDonate";
            this.tsbDonate.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tsbDonate.Text = "Donate";
            this.tsbDonate.ToolTipText = "Support the tool (PayPal)";
            this.tsbDonate.Click += new System.EventHandler(this.tsbDonate_Click);
            //
            // tssLinksSeparator
            //
            this.tssLinksSeparator.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tssLinksSeparator.Name = "tssLinksSeparator";
            //
            // tsbReload
            //
            this.tsbReload.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbReload.Name = "tsbReload";
            this.tsbReload.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tsbReload.Text = "Reload tables";
            this.tsbReload.ToolTipText = "Reload the tables and users of the connected environment";
            this.tsbReload.Click += new System.EventHandler(this.tsbReload_Click);
            //
            // tslConnection
            //
            this.tslConnection.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(212)))), ((int)(((byte)(191)))));
            this.tslConnection.Margin = new System.Windows.Forms.Padding(0, 1, 12, 2);
            this.tslConnection.Name = "tslConnection";
            //
            // pnlCanvas
            //
            this.pnlCanvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.pnlCanvas.Controls.Add(this.splitMain);
            this.pnlCanvas.Controls.Add(this.pnlFilterSpacer);
            this.pnlCanvas.Controls.Add(this.cardFilters);
            this.pnlCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.Location = new System.Drawing.Point(0, 60);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Padding = new System.Windows.Forms.Padding(12);
            this.pnlCanvas.Size = new System.Drawing.Size(1280, 740);
            this.pnlCanvas.TabIndex = 1;
            //
            // splitMain
            //
            this.splitMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(12, 190);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitMain.Panel1
            //
            this.splitMain.Panel1.Controls.Add(this.cardRecords);
            //
            // splitMain.Panel2
            //
            this.splitMain.Panel2.Controls.Add(this.splitBottom);
            this.splitMain.Size = new System.Drawing.Size(1256, 538);
            this.splitMain.SplitterDistance = 290;
            this.splitMain.SplitterWidth = 12;
            this.splitMain.TabIndex = 2;
            //
            // cardRecords
            //
            this.cardRecords.Controls.Add(this.dgvRecords);
            this.cardRecords.Controls.Add(this.pnlRecordActions);
            this.cardRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardRecords.Location = new System.Drawing.Point(0, 0);
            this.cardRecords.Name = "cardRecords";
            this.cardRecords.Size = new System.Drawing.Size(1256, 290);
            this.cardRecords.Step = 2;
            this.cardRecords.Subtitle = "Check the records to restore. Click a row to see its values before deletion";
            this.cardRecords.TabIndex = 0;
            this.cardRecords.Title = "Select the records to restore";
            //
            // dgvRecords
            //
            this.dgvRecords.AllowUserToAddRows = false;
            this.dgvRecords.AllowUserToDeleteRows = false;
            this.dgvRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCheck,
            this.colName,
            this.colRecordId,
            this.colDeletedOn,
            this.colDeletedBy});
            this.dgvRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecords.Location = new System.Drawing.Point(14, 110);
            this.dgvRecords.MultiSelect = false;
            this.dgvRecords.Name = "dgvRecords";
            this.dgvRecords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecords.Size = new System.Drawing.Size(1228, 166);
            this.dgvRecords.TabIndex = 1;
            this.dgvRecords.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRecords_CellValueChanged);
            this.dgvRecords.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvRecords_CurrentCellDirtyStateChanged);
            this.dgvRecords.SelectionChanged += new System.EventHandler(this.dgvRecords_SelectionChanged);
            //
            // colCheck
            //
            this.colCheck.HeaderText = "";
            this.colCheck.Name = "colCheck";
            this.colCheck.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colCheck.Width = 40;
            //
            // colName
            //
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.FillWeight = 120F;
            this.colName.HeaderText = "DELETED RECORD";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            //
            // colRecordId
            //
            this.colRecordId.HeaderText = "GUID";
            this.colRecordId.Name = "colRecordId";
            this.colRecordId.ReadOnly = true;
            this.colRecordId.Width = 290;
            //
            // colDeletedOn
            //
            this.colDeletedOn.HeaderText = "DELETED ON";
            this.colDeletedOn.Name = "colDeletedOn";
            this.colDeletedOn.ReadOnly = true;
            this.colDeletedOn.Width = 150;
            //
            // colDeletedBy
            //
            this.colDeletedBy.HeaderText = "DELETED BY";
            this.colDeletedBy.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDeletedBy.FillWeight = 60F;
            this.colDeletedBy.Name = "colDeletedBy";
            this.colDeletedBy.ReadOnly = true;
            //
            // pnlRecordActions
            //
            this.pnlRecordActions.BackColor = System.Drawing.Color.White;
            this.pnlRecordActions.Controls.Add(this.btnCheckAll);
            this.pnlRecordActions.Controls.Add(this.btnUncheckAll);
            this.pnlRecordActions.Controls.Add(this.lblSummary);
            this.pnlRecordActions.Controls.Add(this.btnRestore);
            this.pnlRecordActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRecordActions.Location = new System.Drawing.Point(14, 66);
            this.pnlRecordActions.Name = "pnlRecordActions";
            this.pnlRecordActions.Size = new System.Drawing.Size(1228, 44);
            this.pnlRecordActions.TabIndex = 0;
            //
            // btnCheckAll
            //
            this.btnCheckAll.Glyph = Dataverse_Record_Recovery.UI.IconKind.Check;
            this.btnCheckAll.Location = new System.Drawing.Point(0, 2);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(104, 32);
            this.btnCheckAll.TabIndex = 0;
            this.btnCheckAll.Text = "Check all";
            this.btnCheckAll.Variant = Dataverse_Record_Recovery.UI.Controls.ButtonVariant.Ghost;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            //
            // btnUncheckAll
            //
            this.btnUncheckAll.Location = new System.Drawing.Point(108, 2);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(96, 32);
            this.btnUncheckAll.TabIndex = 1;
            this.btnUncheckAll.Text = "Uncheck all";
            this.btnUncheckAll.Variant = Dataverse_Record_Recovery.UI.Controls.ButtonVariant.Ghost;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            //
            // lblSummary
            //
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.AutoEllipsis = true;
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblSummary.Location = new System.Drawing.Point(216, 2);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(730, 32);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnRestore
            //
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestore.Enabled = false;
            this.btnRestore.Glyph = Dataverse_Record_Recovery.UI.IconKind.Reload;
            this.btnRestore.Location = new System.Drawing.Point(958, 0);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(270, 36);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "Preview and restore";
            this.btnRestore.Variant = Dataverse_Record_Recovery.UI.Controls.ButtonVariant.Primary;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            //
            // splitBottom
            //
            this.splitBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.splitBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitBottom.Location = new System.Drawing.Point(0, 0);
            this.splitBottom.Name = "splitBottom";
            //
            // splitBottom.Panel1
            //
            this.splitBottom.Panel1.Controls.Add(this.cardDetails);
            //
            // splitBottom.Panel2
            //
            this.splitBottom.Panel2.Controls.Add(this.cardLog);
            this.splitBottom.Size = new System.Drawing.Size(1256, 236);
            this.splitBottom.SplitterDistance = 622;
            this.splitBottom.SplitterWidth = 12;
            this.splitBottom.TabIndex = 0;
            //
            // cardDetails
            //
            this.cardDetails.Controls.Add(this.dgvDetails);
            this.cardDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardDetails.Location = new System.Drawing.Point(0, 0);
            this.cardDetails.Name = "cardDetails";
            this.cardDetails.Size = new System.Drawing.Size(622, 236);
            this.cardDetails.Subtitle = "Click a deleted record to see what the audit history kept";
            this.cardDetails.TabIndex = 0;
            this.cardDetails.Title = "Values before deletion";
            //
            // dgvDetails
            //
            this.dgvDetails.AllowUserToAddRows = false;
            this.dgvDetails.AllowUserToDeleteRows = false;
            this.dgvDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDetailColumn,
            this.colDetailLogicalName,
            this.colDetailValue});
            this.dgvDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetails.Location = new System.Drawing.Point(14, 66);
            this.dgvDetails.Name = "dgvDetails";
            this.dgvDetails.ReadOnly = true;
            this.dgvDetails.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetails.Size = new System.Drawing.Size(594, 156);
            this.dgvDetails.TabIndex = 0;
            //
            // colDetailColumn
            //
            this.colDetailColumn.HeaderText = "COLUMN";
            this.colDetailColumn.Name = "colDetailColumn";
            this.colDetailColumn.ReadOnly = true;
            this.colDetailColumn.Width = 180;
            //
            // colDetailLogicalName
            //
            this.colDetailLogicalName.HeaderText = "LOGICAL NAME";
            this.colDetailLogicalName.Name = "colDetailLogicalName";
            this.colDetailLogicalName.ReadOnly = true;
            this.colDetailLogicalName.Width = 160;
            //
            // colDetailValue
            //
            this.colDetailValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDetailValue.HeaderText = "VALUE";
            this.colDetailValue.Name = "colDetailValue";
            this.colDetailValue.ReadOnly = true;
            //
            // cardLog
            //
            this.cardLog.Controls.Add(this.dgvLog);
            this.cardLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLog.Location = new System.Drawing.Point(0, 0);
            this.cardLog.Name = "cardLog";
            this.cardLog.Size = new System.Drawing.Size(622, 236);
            this.cardLog.Subtitle = "Result of the last recovery, record by record";
            this.cardLog.TabIndex = 0;
            this.cardLog.Title = "Recovery log";
            //
            // dgvLog
            //
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLogResult,
            this.colLogRole,
            this.colLogTable,
            this.colLogName,
            this.colLogMessage});
            this.dgvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLog.Location = new System.Drawing.Point(14, 66);
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.ReadOnly = true;
            this.dgvLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLog.ShowCellToolTips = true;
            this.dgvLog.Size = new System.Drawing.Size(594, 156);
            this.dgvLog.TabIndex = 0;
            //
            // colLogResult
            //
            this.colLogResult.HeaderText = "RESULT";
            this.colLogResult.Name = "colLogResult";
            this.colLogResult.ReadOnly = true;
            this.colLogResult.Width = 150;
            //
            // colLogRole
            //
            this.colLogRole.HeaderText = "ROLE";
            this.colLogRole.Name = "colLogRole";
            this.colLogRole.ReadOnly = true;
            this.colLogRole.Width = 100;
            //
            // colLogTable
            //
            this.colLogTable.HeaderText = "TABLE";
            this.colLogTable.Name = "colLogTable";
            this.colLogTable.ReadOnly = true;
            this.colLogTable.Width = 100;
            //
            // colLogName
            //
            this.colLogName.HeaderText = "RECORD";
            this.colLogName.Name = "colLogName";
            this.colLogName.ReadOnly = true;
            this.colLogName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLogName.FillWeight = 60F;
            //
            // colLogMessage
            //
            this.colLogMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLogMessage.HeaderText = "MESSAGE";
                        this.colLogMessage.Name = "colLogMessage";
            this.colLogMessage.ReadOnly = true;
            //
            // pnlFilterSpacer
            //
            this.pnlFilterSpacer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterSpacer.Location = new System.Drawing.Point(12, 178);
            this.pnlFilterSpacer.Name = "pnlFilterSpacer";
            this.pnlFilterSpacer.Size = new System.Drawing.Size(1256, 12);
            this.pnlFilterSpacer.TabIndex = 1;
            //
            // cardFilters
            //
            this.cardFilters.Controls.Add(this.lblTable);
            this.cardFilters.Controls.Add(this.cmbTable);
            this.cardFilters.Controls.Add(this.lblUser);
            this.cardFilters.Controls.Add(this.cmbUser);
            this.cardFilters.Controls.Add(this.lblFrom);
            this.cardFilters.Controls.Add(this.dtpFrom);
            this.cardFilters.Controls.Add(this.lblTo);
            this.cardFilters.Controls.Add(this.dtpTo);
            this.cardFilters.Controls.Add(this.lblMaxRecords);
            this.cardFilters.Controls.Add(this.nudMaxRecords);
            this.cardFilters.Controls.Add(this.btnSearch);
            this.cardFilters.Controls.Add(this.chkAuditedOnly);
            this.cardFilters.Controls.Add(this.chkRelated);
            this.cardFilters.Controls.Add(this.nudWindow);
            this.cardFilters.Controls.Add(this.lblWindow);
            this.cardFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.cardFilters.Location = new System.Drawing.Point(12, 12);
            this.cardFilters.Name = "cardFilters";
            this.cardFilters.Size = new System.Drawing.Size(1256, 166);
            this.cardFilters.Step = 1;
            this.cardFilters.Subtitle = "Choose a table, who deleted the records and when (local time), then search the audit history";
            this.cardFilters.TabIndex = 0;
            this.cardFilters.Title = "Find deleted records";
            //
            // lblTable
            //
            this.lblTable.AutoSize = true;
            this.lblTable.BackColor = System.Drawing.Color.White;
            this.lblTable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblTable.Location = new System.Drawing.Point(18, 70);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(34, 15);
            this.lblTable.TabIndex = 0;
            this.lblTable.Text = "Table";
            //
            // cmbTable
            //
            this.cmbTable.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbTable.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Location = new System.Drawing.Point(18, 90);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(280, 23);
            this.cmbTable.TabIndex = 1;
            //
            // lblUser
            //
            this.lblUser.AutoSize = true;
            this.lblUser.BackColor = System.Drawing.Color.White;
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblUser.Location = new System.Drawing.Point(312, 70);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(62, 15);
            this.lblUser.TabIndex = 2;
            this.lblUser.Text = "Deleted by";
            //
            // cmbUser
            //
            this.cmbUser.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbUser.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbUser.FormattingEnabled = true;
            this.cmbUser.Location = new System.Drawing.Point(312, 90);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(260, 23);
            this.cmbUser.TabIndex = 3;
            //
            // lblFrom
            //
            this.lblFrom.AutoSize = true;
            this.lblFrom.BackColor = System.Drawing.Color.White;
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblFrom.Location = new System.Drawing.Point(586, 70);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(75, 15);
            this.lblFrom.TabIndex = 4;
            this.lblFrom.Text = "Deleted from";
            //
            // dtpFrom
            //
            this.dtpFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFrom.Location = new System.Drawing.Point(586, 90);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(150, 23);
            this.dtpFrom.TabIndex = 5;
            //
            // lblTo
            //
            this.lblTo.AutoSize = true;
            this.lblTo.BackColor = System.Drawing.Color.White;
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblTo.Location = new System.Drawing.Point(750, 70);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(19, 15);
            this.lblTo.TabIndex = 6;
            this.lblTo.Text = "To";
            //
            // dtpTo
            //
            this.dtpTo.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTo.Location = new System.Drawing.Point(750, 90);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(150, 23);
            this.dtpTo.TabIndex = 7;
            //
            // lblMaxRecords
            //
            this.lblMaxRecords.AutoSize = true;
            this.lblMaxRecords.BackColor = System.Drawing.Color.White;
            this.lblMaxRecords.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(115)))), ((int)(((byte)(133)))));
            this.lblMaxRecords.Location = new System.Drawing.Point(914, 70);
            this.lblMaxRecords.Name = "lblMaxRecords";
            this.lblMaxRecords.Size = new System.Drawing.Size(72, 15);
            this.lblMaxRecords.TabIndex = 8;
            this.lblMaxRecords.Text = "Max records";
            //
            // nudMaxRecords
            //
            this.nudMaxRecords.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMaxRecords.Location = new System.Drawing.Point(914, 90);
            this.nudMaxRecords.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.nudMaxRecords.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxRecords.Name = "nudMaxRecords";
            this.nudMaxRecords.Size = new System.Drawing.Size(84, 23);
            this.nudMaxRecords.TabIndex = 9;
            this.nudMaxRecords.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            //
            // btnSearch
            //
            this.btnSearch.Glyph = Dataverse_Record_Recovery.UI.IconKind.Search;
            this.btnSearch.Location = new System.Drawing.Point(1016, 85);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(120, 34);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "Search";
            this.btnSearch.Variant = Dataverse_Record_Recovery.UI.Controls.ButtonVariant.Primary;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // chkAuditedOnly
            //
            this.chkAuditedOnly.AutoSize = true;
            this.chkAuditedOnly.BackColor = System.Drawing.Color.White;
            this.chkAuditedOnly.Checked = true;
            this.chkAuditedOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAuditedOnly.Location = new System.Drawing.Point(20, 128);
            this.chkAuditedOnly.Name = "chkAuditedOnly";
            this.chkAuditedOnly.Size = new System.Drawing.Size(180, 19);
            this.chkAuditedOnly.TabIndex = 11;
            this.chkAuditedOnly.Text = "Only tables with auditing on";
            this.chkAuditedOnly.UseVisualStyleBackColor = false;
            this.chkAuditedOnly.CheckedChanged += new System.EventHandler(this.chkAuditedOnly_CheckedChanged);
            //
            // chkRelated
            //
            this.chkRelated.AutoSize = true;
            this.chkRelated.BackColor = System.Drawing.Color.White;
            this.chkRelated.Checked = true;
            this.chkRelated.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRelated.Location = new System.Drawing.Point(314, 128);
            this.chkRelated.Name = "chkRelated";
            this.chkRelated.Size = new System.Drawing.Size(330, 19);
            this.chkRelated.TabIndex = 12;
            this.chkRelated.Text = "Also restore related records deleted in cascade, within";
            this.chkRelated.UseVisualStyleBackColor = false;
            this.chkRelated.CheckedChanged += new System.EventHandler(this.chkRelated_CheckedChanged);
            //
            // nudWindow
            //
            this.nudWindow.Increment = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudWindow.Location = new System.Drawing.Point(650, 126);
            this.nudWindow.Maximum = new decimal(new int[] {
            86400,
            0,
            0,
            0});
            this.nudWindow.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWindow.Name = "nudWindow";
            this.nudWindow.Size = new System.Drawing.Size(70, 23);
            this.nudWindow.TabIndex = 13;
            this.nudWindow.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            //
            // lblWindow
            //
            this.lblWindow.AutoSize = true;
            this.lblWindow.BackColor = System.Drawing.Color.White;
            this.lblWindow.Location = new System.Drawing.Point(726, 129);
            this.lblWindow.Name = "lblWindow";
            this.lblWindow.Size = new System.Drawing.Size(200, 15);
            this.lblWindow.TabIndex = 14;
            this.lblWindow.Text = "seconds before or after each deletion";
            //
            // RecordRecoveryControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.pnlCanvas);
            this.Controls.Add(this.tsHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "RecordRecoveryControl";
            this.Size = new System.Drawing.Size(1280, 800);
            this.OnCloseTool += new System.EventHandler(this.RecordRecoveryControl_OnCloseTool);
            this.Load += new System.EventHandler(this.RecordRecoveryControl_Load);
            this.tsHeader.ResumeLayout(false);
            this.tsHeader.PerformLayout();
            this.pnlCanvas.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.cardRecords.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecords)).EndInit();
            this.pnlRecordActions.ResumeLayout(false);
            this.splitBottom.Panel1.ResumeLayout(false);
            this.splitBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitBottom)).EndInit();
            this.splitBottom.ResumeLayout(false);
            this.cardDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).EndInit();
            this.cardLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            this.cardFilters.ResumeLayout(false);
            this.cardFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxRecords)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWindow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsHeader;
        private System.Windows.Forms.ToolStripLabel tslLogo;
        private System.Windows.Forms.ToolStripLabel tslSubtitle;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssHeaderSeparator;
        private System.Windows.Forms.ToolStripButton tsbAuthor;
        private System.Windows.Forms.ToolStripButton tsbDonate;
        private System.Windows.Forms.ToolStripSeparator tssLinksSeparator;
        private System.Windows.Forms.ToolStripButton tsbReload;
        private System.Windows.Forms.ToolStripLabel tslConnection;
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.SplitContainer splitMain;
        private Dataverse_Record_Recovery.UI.Controls.CardPanel cardRecords;
        private System.Windows.Forms.DataGridView dgvRecords;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecordId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeletedOn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeletedBy;
        private System.Windows.Forms.Panel pnlRecordActions;
        private Dataverse_Record_Recovery.UI.Controls.ModernButton btnCheckAll;
        private Dataverse_Record_Recovery.UI.Controls.ModernButton btnUncheckAll;
        private System.Windows.Forms.Label lblSummary;
        private Dataverse_Record_Recovery.UI.Controls.ModernButton btnRestore;
        private System.Windows.Forms.SplitContainer splitBottom;
        private Dataverse_Record_Recovery.UI.Controls.CardPanel cardDetails;
        private System.Windows.Forms.DataGridView dgvDetails;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDetailColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDetailLogicalName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDetailValue;
        private Dataverse_Record_Recovery.UI.Controls.CardPanel cardLog;
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogRole;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLogMessage;
        private System.Windows.Forms.Panel pnlFilterSpacer;
        private Dataverse_Record_Recovery.UI.Controls.CardPanel cardFilters;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.ComboBox cmbTable;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.ComboBox cmbUser;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Label lblMaxRecords;
        private System.Windows.Forms.NumericUpDown nudMaxRecords;
        private Dataverse_Record_Recovery.UI.Controls.ModernButton btnSearch;
        private System.Windows.Forms.CheckBox chkAuditedOnly;
        private System.Windows.Forms.CheckBox chkRelated;
        private System.Windows.Forms.NumericUpDown nudWindow;
        private System.Windows.Forms.Label lblWindow;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
