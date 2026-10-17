using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dataverse_Record_Recovery.Localization;
using Dataverse_Record_Recovery.Models;
using Dataverse_Record_Recovery.Services;
using Dataverse_Record_Recovery.UI;
using Dataverse_Record_Recovery.UI.Controls;

namespace Dataverse_Record_Recovery.Forms
{
    /// <summary>
    /// Shows the records that are about to be restored, with their column values, their missing parents
    /// and their deleted related records, and asks the user to approve the recovery
    /// </summary>
    [DesignerCategory("Code")]
    public class RecoveryConfirmationDialog : ThemedDialog
    {
        private readonly RecoveryPlan plan;
        private readonly MetadataCache metadata;
        private readonly Dictionary<RecoveryItem, ModernTreeNode> nodes = new Dictionary<RecoveryItem, ModernTreeNode>();
        private readonly Dictionary<RecoveryItem, int> restoreOrder = new Dictionary<RecoveryItem, int>();

        private Label lblSummary;
        private SplitContainer split;
        private ModernTreeView tvRecords;
        private CardPanel cardRecord;
        private Label lblRelation;
        private DataGridView dgvAttributes;
        private CheckBox chkPreserveCreatedOn;
        private CheckBox chkBypass;
        private CheckBox chkConfirm;
        private ModernButton btnRestore;

        public RecoveryConfirmationDialog(RecoveryPlan plan, MetadataCache metadata, string environmentName, bool preserveCreatedOn)
            : base(Strings.ConfirmTitle, Strings.ConfirmSubtitle(environmentName))
        {
            this.plan = plan;
            this.metadata = metadata;

            BuildLayout(environmentName);
            chkPreserveCreatedOn.Checked = preserveCreatedOn;
            FillTree();
            RefreshSummary();
        }

        public RecoveryOptions Options => new RecoveryOptions
        {
            PreserveCreatedOn = chkPreserveCreatedOn.Checked,
            BypassCustomLogic = chkBypass.Checked
        };

        private void BuildLayout(string environmentName)
        {
            Size = new Size(1200, 780);
            MinimumSize = new Size(920, 620);
            Resizable = true;

            lblSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Font = Theme.Semibold,
                ForeColor = Theme.Text
            };

            // Left: records of the plan
            tvRecords = new ModernTreeView { Dock = DockStyle.Fill };
            tvRecords.AfterCheck += tvRecords_AfterCheck;
            tvRecords.SelectedNodeChanged += tvRecords_SelectedNodeChanged;

            var btnExpand = new ModernButton
            {
                Text = Strings.ExpandAll,
                Variant = ButtonVariant.Ghost,
                Glyph = IconKind.ChevronDown,
                AutoSize = true,
                Height = 30,
                Margin = new Padding(0, 0, 4, 0)
            };
            btnExpand.Click += (s, e) => tvRecords.ExpandAll();
            var btnCollapse = new ModernButton
            {
                Text = Strings.CollapseAll,
                Variant = ButtonVariant.Ghost,
                Glyph = IconKind.ChevronRight,
                AutoSize = true,
                Height = 30,
                Margin = new Padding(0)
            };
            btnCollapse.Click += (s, e) => tvRecords.CollapseAll();
            var treeTools = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Theme.Card,
                WrapContents = false
            };
            treeTools.Controls.Add(btnExpand);
            treeTools.Controls.Add(btnCollapse);

            var cardTree = new CardPanel
            {
                Dock = DockStyle.Fill,
                Title = Strings.TreeTitle,
                Subtitle = Strings.TreeSubtitle
            };
            cardTree.Controls.Add(tvRecords);
            cardTree.Controls.Add(treeTools);

            // Right: column values of the record picked in the tree
            dgvAttributes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvAttributes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColColumn, FillWeight = 70 });
            dgvAttributes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColLogicalName, FillWeight = 60 });
            dgvAttributes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColPreviousValue, FillWeight = 100 });
            dgvAttributes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColRestored, FillWeight = 46 });
            dgvAttributes.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColNote, FillWeight = 110 });
            Theme.StyleGrid(dgvAttributes, true);
            dgvAttributes.Columns[1].DefaultCellStyle.Font = Theme.Mono;

            lblRelation = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Theme.Card,
                ForeColor = Theme.Muted,
                AutoEllipsis = true
            };

            cardRecord = new CardPanel { Dock = DockStyle.Fill, Title = Strings.RecordDetails };
            cardRecord.Controls.Add(dgvAttributes);
            cardRecord.Controls.Add(lblRelation);

            split = new SplitContainer { Dock = DockStyle.Fill, SplitterWidth = 12, BackColor = Theme.Canvas };
            split.Panel1.Controls.Add(cardTree);
            split.Panel2.Controls.Add(cardRecord);

            // Bottom: options and approval
            chkPreserveCreatedOn = CreateOption(Strings.OptionPreserveCreatedOn, 14);
            chkBypass = CreateOption(Strings.OptionBypass, 38);
            chkConfirm = CreateOption(Strings.OptionConfirm(environmentName), 70);
            chkConfirm.Font = Theme.Semibold;
            chkConfirm.ForeColor = Theme.Accent;
            chkConfirm.CheckedChanged += (s, e) => RefreshSummary();

            var cardOptions = new CardPanel { Dock = DockStyle.Bottom, Height = 108 };
            cardOptions.Controls.AddRange(new Control[] { chkPreserveCreatedOn, chkBypass, chkConfirm });

            Body.Controls.Add(split);
            Body.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 12 });
            Body.Controls.Add(cardOptions);
            Body.Controls.Add(lblSummary);

            var btnCancel = AddFooterButton(Strings.Cancel, ButtonVariant.Secondary, DialogResult.Cancel);
            btnRestore = AddFooterButton(Strings.RestoreCount(0), ButtonVariant.Primary, DialogResult.OK, IconKind.Reload);
            btnRestore.Enabled = false;

            AcceptButton = btnRestore;
            CancelButton = btnCancel;
        }

        private static CheckBox CreateOption(string text, int top)
        {
            return new CheckBox
            {
                Text = text,
                AutoSize = true,
                Location = new Point(18, top),
                BackColor = Theme.Card,
                ForeColor = Theme.Text
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Only valid once the dialog has its final size
            split.SplitterDistance = split.Width * 9 / 20;
            tvRecords.Focus();
        }

        /// <summary>
        /// Builds the tree in the direction of the relationships: a missing parent is shown above the record
        /// that references it, a record deleted in cascade below its parent. Siblings follow the restore order.
        /// </summary>
        private void FillTree()
        {
            for (var i = 0; i < plan.OrderedItems.Count; i++)
            {
                restoreOrder[plan.OrderedItems[i]] = i;
            }
            foreach (var item in plan.OrderedItems)
            {
                nodes[item] = CreateNode(item);
            }

            var roots = new List<ModernTreeNode>();
            foreach (var item in plan.OrderedItems)
            {
                var parent = GetDisplayParent(item);
                if (parent == null)
                {
                    roots.Add(nodes[item]);
                }
                else
                {
                    nodes[parent].Add(nodes[item]);
                }
            }

            tvRecords.SetNodes(roots);
            if (roots.Count > 0)
            {
                tvRecords.SelectedNode = nodes.Where(n => n.Key.Role == RecoveryRole.Selected).Select(n => n.Value).FirstOrDefault() ?? roots[0];
            }
        }

        private RecoveryItem GetDisplayParent(RecoveryItem item)
        {
            var candidate = FindDisplayParent(item);

            // Safety net: a node is never placed below itself
            for (var ancestor = candidate; ancestor != null; ancestor = FindDisplayParent(ancestor))
            {
                if (ancestor == item)
                {
                    return null;
                }
            }
            return candidate;
        }

        private RecoveryItem FindDisplayParent(RecoveryItem item)
        {
            var missingParent = item.Children
                .Where(c => c.Role == RecoveryRole.MissingParent && nodes.ContainsKey(c))
                .OrderBy(c => restoreOrder[c])
                .FirstOrDefault();
            if (missingParent != null)
            {
                return missingParent;
            }
            return item.Role == RecoveryRole.RelatedChild && item.Parent != null && nodes.ContainsKey(item.Parent) ? item.Parent : null;
        }

        private ModernTreeNode CreateNode(RecoveryItem item)
        {
            var record = item.Record;
            var table = metadata.GetDisplayName(record.EntityLogicalName);
            var roleColor = GetRoleColor(item.Role);

            var node = new ModernTreeNode(GetName(record))
            {
                Tag = item,
                Caption = string.IsNullOrEmpty(item.Relation) ? $"{table}  ·  {record.RecordId}" : $"{table}  ·  {item.Relation}",
                Initials = GetInitials(table),
                AccentColor = roleColor,
                Bold = item.Role == RecoveryRole.Selected,
                Checked = item.Include,
                ToolTip = record.RecordId.ToString()
            };

            node.Pills.Add(new NodePill(GetRoleLabel(item.Role), roleColor, Theme.Blend(Color.White, roleColor, 0.12f)));
            if (item.Warnings.Count > 0)
            {
                node.Pills.Add(new NodePill(Strings.WarningsTag(item.Warnings.Count), Theme.WarningText, Theme.WarningSoft));
            }
            return node;
        }

        private void tvRecords_AfterCheck(object sender, ModernTreeNodeEventArgs e)
        {
            var item = (RecoveryItem)e.Node.Tag;
            item.Include = e.Node.Checked;

            // Records brought by a record follow it, checking a record does not check its parent back
            foreach (var child in item.Children)
            {
                ModernTreeNode node;
                if (nodes.TryGetValue(child, out node))
                {
                    tvRecords.SetChecked(node, e.Node.Checked);
                }
            }

            if (e.ByUser)
            {
                RefreshSummary();
            }
        }

        private void tvRecords_SelectedNodeChanged(object sender, EventArgs e)
        {
            var item = tvRecords.SelectedNode?.Tag as RecoveryItem;
            if (item == null)
            {
                return;
            }
            var record = item.Record;

            cardRecord.Title = $"{metadata.GetDisplayName(record.EntityLogicalName)}: {GetName(record)}";
            cardRecord.Subtitle = Strings.RecordSubtitle(record.RecordId, record.DeletedOn.ToLocalTime(), record.DeletedBy?.Name);

            lblRelation.Text = string.IsNullOrEmpty(item.Relation)
                ? Strings.SelectedRecordMessage
                : $"{GetRoleLabel(item.Role)}: {item.Relation}";

            dgvAttributes.Rows.Clear();
            foreach (var attribute in item.Attributes)
            {
                var index = dgvAttributes.Rows.Add(
                    attribute.DisplayName,
                    attribute.LogicalName,
                    attribute.Value,
                    attribute.WillRestore ? Strings.Yes : Strings.No,
                    attribute.Note ?? string.Empty);
                if (attribute.WillRestore)
                {
                    continue;
                }

                var color = attribute.MissingReference ? Theme.WarningText : Theme.Subtle;
                var style = dgvAttributes.Rows[index].DefaultCellStyle;
                style.ForeColor = color;
                style.SelectionForeColor = color;
            }
            dgvAttributes.ClearSelection();
        }

        private void RefreshSummary()
        {
            var included = plan.Items.Where(i => i.Include).ToList();
            var selected = included.Count(i => i.Role == RecoveryRole.Selected);
            var parents = included.Count(i => i.Role == RecoveryRole.MissingParent);
            var related = included.Count(i => i.Role == RecoveryRole.RelatedChild);
            var existing = plan.ExistingCount;
            var warnings = included.Sum(i => i.Warnings.Count);

            lblSummary.Text = Strings.ConfirmSummary(included.Count, selected, parents, related, existing, warnings);
            var associations = included.Sum(i => i.Associations.Count(a => a.Available));
            var relinks = included.Sum(i => i.Relinks.Count);
            if (associations + relinks > 0)
            {
                lblSummary.Text += "  " + Strings.LinksSummary(associations, relinks);
            }
            lblSummary.ForeColor = warnings > 0 ? Theme.WarningText : Theme.Text;

            btnRestore.Text = Strings.RestoreCount(included.Count);
            btnRestore.Enabled = chkConfirm.Checked && included.Count > 0;
        }

        private static Color GetRoleColor(RecoveryRole role)
        {
            switch (role)
            {
                case RecoveryRole.MissingParent:
                    return Theme.Master;

                case RecoveryRole.RelatedChild:
                    return Theme.TealDark;

                default:
                    return Theme.Accent;
            }
        }

        private static string GetRoleLabel(RecoveryRole role)
        {
            switch (role)
            {
                case RecoveryRole.MissingParent:
                    return Strings.RoleMissingParent;

                case RecoveryRole.RelatedChild:
                    return Strings.RoleRelated;

                default:
                    return Strings.RoleSelected;
            }
        }

        private static string GetInitials(string table)
        {
            var words = (table ?? string.Empty).Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return "?";
            }
            return words.Length == 1
                ? words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpperInvariant()
                : (words[0].Substring(0, 1) + words[1].Substring(0, 1)).ToUpperInvariant();
        }

        private static string GetName(DeletedRecord record)
        {
            return string.IsNullOrEmpty(record.Name) ? record.RecordId.ToString() : record.Name;
        }
    }
}
