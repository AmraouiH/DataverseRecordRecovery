using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Dataverse_Record_Recovery.UI
{
    /// <summary>
    /// Visual identity shared with the other tools (MergeIt...): palette, fonts and shared drawing helpers.
    /// No third-party UI library: everything is drawn with GDI+ so the plugin ships as a single DLL.
    /// </summary>
    internal static class Theme
    {
        #region Palette

        public static readonly Color Canvas = Color.FromArgb(243, 245, 249);
        public static readonly Color Card = Color.White;
        public static readonly Color Border = Color.FromArgb(226, 231, 239);
        public static readonly Color Divider = Color.FromArgb(238, 241, 246);
        public static readonly Color Text = Color.FromArgb(30, 36, 51);
        public static readonly Color Muted = Color.FromArgb(107, 115, 133);
        public static readonly Color Subtle = Color.FromArgb(160, 167, 180);

        public static readonly Color Accent = Color.FromArgb(91, 79, 232);
        public static readonly Color AccentHover = Color.FromArgb(77, 64, 214);
        public static readonly Color AccentPressed = Color.FromArgb(63, 52, 189);
        public static readonly Color AccentSoft = Color.FromArgb(238, 236, 254);
        public static readonly Color Teal = Color.FromArgb(45, 212, 191);
        public static readonly Color TealDark = Color.FromArgb(13, 148, 136);

        public static readonly Color HeaderStart = Color.FromArgb(23, 21, 59);
        public static readonly Color HeaderEnd = Color.FromArgb(52, 44, 138);

        public static readonly Color Master = Color.FromArgb(37, 99, 235);
        public static readonly Color MasterSoft = Color.FromArgb(230, 238, 255);
        public static readonly Color MasterSelected = Color.FromArgb(207, 222, 255);
        public static readonly Color Slave = Color.FromArgb(234, 88, 12);
        public static readonly Color SlaveSoft = Color.FromArgb(255, 238, 227);
        public static readonly Color SlaveSelected = Color.FromArgb(255, 217, 194);

        public static readonly Color Diff = Color.FromArgb(245, 158, 11);
        public static readonly Color DiffSoft = Color.FromArgb(255, 250, 238);
        public static readonly Color SameBack = Color.FromArgb(250, 251, 253);
        public static readonly Color Success = Color.FromArgb(22, 163, 74);
        public static readonly Color SuccessText = Color.FromArgb(15, 122, 61);
        public static readonly Color Danger = Color.FromArgb(185, 28, 28);
        public static readonly Color WarningSoft = Color.FromArgb(255, 244, 229);
        public static readonly Color WarningText = Color.FromArgb(138, 75, 0);
        public static readonly Color DisabledFill = Color.FromArgb(203, 208, 219);

        #endregion

        #region Fonts

        public static readonly Font Base = new Font("Segoe UI", 9F);
        public static readonly Font Semibold = new Font("Segoe UI Semibold", 9F);
        public static readonly Font Small = new Font("Segoe UI", 8F);
        public static readonly Font SmallBold = new Font("Segoe UI Semibold", 7.5F);
        public static readonly Font CardTitle = new Font("Segoe UI Semibold", 11F);
        public static readonly Font HeaderTitle = new Font("Segoe UI Semibold", 13F);
        public static readonly Font Stat = new Font("Segoe UI Semibold", 15F);
        public static readonly Font Mono = new Font("Consolas", 8.5F);
        public static readonly Font Strike = new Font("Segoe UI", 9F, FontStyle.Strikeout);

        #endregion

        public static GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0.5f)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color Blend(Color from, Color to, float amount)
        {
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * amount),
                (int)(from.G + (to.G - from.G) * amount),
                (int)(from.B + (to.B - from.B) * amount));
        }

        /// <summary>
        /// Flat, airy grid style shared by every DataGridView of the tool.
        /// </summary>
        public static void StyleGrid(DataGridView grid, bool alternateRows)
        {
            EnableDoubleBuffer(grid);
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Card;
            grid.GridColor = Divider;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 30;

            var header = grid.ColumnHeadersDefaultCellStyle;
            header.BackColor = Color.FromArgb(247, 248, 251);
            header.ForeColor = Muted;
            header.SelectionBackColor = header.BackColor;
            header.SelectionForeColor = Muted;
            header.Font = SmallBold;
            header.Padding = new Padding(6, 0, 6, 0);
            header.Alignment = DataGridViewContentAlignment.MiddleLeft;
            header.WrapMode = DataGridViewTriState.False;

            var cell = grid.DefaultCellStyle;
            cell.BackColor = Card;
            cell.ForeColor = Text;
            cell.Font = Base;
            cell.SelectionBackColor = AccentSoft;
            cell.SelectionForeColor = Text;
            cell.Padding = new Padding(6, 0, 6, 0);

            if (alternateRows)
            {
                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            }
        }

        /// <summary>
        /// Paints a data cell without the native borders (background + content), then a single light divider.
        /// Keeps the grid perfectly flat whatever the system theme.
        /// </summary>
        public static void PaintFlatCell(DataGridViewCellPaintingEventArgs e, DataGridView grid, bool paintContent = true)
        {
            // The Graphics object is shared by all cells: anti-aliasing left on by a custom drawing
            // would blur the rectangle edges and draw 1px seams between cells.
            e.Graphics.SmoothingMode = SmoothingMode.None;

            var parts = DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground;
            if (paintContent) parts |= DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground;
            e.Paint(e.CellBounds, parts);
            PaintDivider(e.Graphics, e.CellBounds, grid.GridColor);
            e.Handled = true;
        }

        public static void PaintDivider(Graphics g, Rectangle cellBounds, Color color)
        {
            var previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;
            using (var pen = new Pen(color))
            {
                g.DrawLine(pen, cellBounds.Left, cellBounds.Bottom - 1, cellBounds.Right, cellBounds.Bottom - 1);
            }
            g.SmoothingMode = previous;
        }

        public static void EnableDoubleBuffer(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(control, true);
        }

        /// <summary>
        /// Native placeholder text (cue banner) for a single-line TextBox.
        /// </summary>
        public static void SetCue(TextBox box, string text)
        {
            SendMessage(box.Handle, 0x1501 /* EM_SETCUEBANNER */, (IntPtr)1, text);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        /// <summary>
        /// The Record Recovery logo: a deleted record (violet) brought back (teal, restore arrow).
        /// </summary>
        public static Bitmap CreateLogo(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var s = size / 16f;
                using (var teal = new SolidBrush(Color.FromArgb(235, Teal)))
                using (var violet = new SolidBrush(Color.FromArgb(225, 139, 128, 255)))
                {
                    g.FillEllipse(violet, 1 * s, 7 * s, 8 * s, 8 * s);
                    g.FillEllipse(teal, 4 * s, 1 * s, 11 * s, 11 * s);
                }
                LineIcons.Draw(g, IconKind.Reload, new RectangleF(5 * s, 2 * s, 9 * s, 9 * s), Color.White);
            }
            return bitmap;
        }

        /// <summary>
        /// Small filled dot used as a menu image (Master / Slave).
        /// </summary>
        public static Bitmap CreateDot(Color color, int size = 16)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(color))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(brush, size * 0.25f, size * 0.25f, size * 0.5f, size * 0.5f);
            }
            return bitmap;
        }
    }
}
