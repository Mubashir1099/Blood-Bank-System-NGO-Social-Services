using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class ChartForm : Form
    {
        private readonly BloodInventoryService _inventoryService = new();
        private readonly BloodRequestService _requestService = new();
        private readonly DonorService _donorService = new();

        private TabControl tabControl;
        private Panel pnlInventoryChart, pnlRequestChart, pnlDonorChart;
        private Button btnRefresh, btnClose;
        private Label lblStatus;

        // Chart data
        private Dictionary<string, int> _inventorySummary = new();
        private Dictionary<string, int> _requestStatus = new();
        private Dictionary<string, int> _donorsByBloodGroup = new();

        // Blood group colors
        private readonly Color[] ChartColors = {
            Color.FromArgb(220, 53, 69),   // A+
            Color.FromArgb(255, 99, 71),   // A-
            Color.FromArgb(13, 110, 253),  // B+
            Color.FromArgb(30, 144, 255),  // B-
            Color.FromArgb(25, 135, 84),   // O+
            Color.FromArgb(60, 179, 113),  // O-
            Color.FromArgb(111, 66, 193),  // AB+
            Color.FromArgb(147, 112, 219)  // AB-
        };

        public ChartForm()
        {
            InitializeComponents();
            LoadChartData();
        }

        private void InitializeComponents()
        {
            this.Text = "Blood Bank — Charts & Analytics";
            this.Size = new Size(1050, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.MinimumSize = new Size(900, 600);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(180, 0, 0) };
            header.Controls.Add(new Label
            {
                Text = "📊  Charts & Analytics",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(14, 14)
            });

            // Tab Control
            tabControl = new TabControl
            {
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.None,
                Location = new Point(10, 65),
            };

            // Tab 1 — Blood Inventory Pie Chart
            pnlInventoryChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlInventoryChart.Paint += DrawInventoryPieChart;
            var tab1 = new TabPage("🩸  Blood Inventory") { BackColor = Color.White };
            tab1.Controls.Add(pnlInventoryChart);

            // Tab 2 — Request Status Bar Chart
            pnlRequestChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlRequestChart.Paint += DrawRequestBarChart;
            var tab2 = new TabPage("📋  Request Status") { BackColor = Color.White };
            tab2.Controls.Add(pnlRequestChart);

            // Tab 3 — Donors by Blood Group Bar Chart
            pnlDonorChart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlDonorChart.Paint += DrawDonorBarChart;
            var tab3 = new TabPage("🧑‍🤝‍🧑  Donors by Group") { BackColor = Color.White };
            tab3.Controls.Add(pnlDonorChart);

            tabControl.TabPages.AddRange(new[] { tab1, tab2, tab3 });
            tabControl.SelectedIndexChanged += (s, e) => tabControl.SelectedTab?.Invalidate(true);

            // Bottom bar
            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.White };
            btnRefresh = MakeBtn("🔄  Refresh", Color.FromArgb(46, 204, 113), new Point(14, 8), new Size(130, 34));
            btnRefresh.Click += (s, e) => LoadChartData();
            btnClose = MakeBtn("✖  Close", Color.FromArgb(149, 165, 166), new Point(155, 8), new Size(100, 34));
            btnClose.Click += (s, e) => Close();
            lblStatus = new Label
            {
                Text = "Loading...", Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray, AutoSize = true, Location = new Point(270, 17)
            };
            btnBar.Controls.AddRange(new Control[] { btnRefresh, btnClose, lblStatus });

            this.Controls.Add(header);
            this.Controls.Add(tabControl);
            this.Controls.Add(btnBar);

            tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 130);
            this.Resize += (s, e) => tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 130);
        }

        private void LoadChartData()
        {
            try
            {
                lblStatus.Text = "Loading data...";
                lblStatus.ForeColor = Color.Gray;

                // Blood inventory summary
                _inventorySummary = _inventoryService.GetBloodGroupSummary();

                // Request status counts
                _requestStatus = new Dictionary<string, int>
                {
                    ["Pending"]  = _requestService.GetRequestsByStatus("Pending").Count,
                    ["Approved"] = _requestService.GetRequestsByStatus("Approved").Count,
                    ["Rejected"] = _requestService.GetRequestsByStatus("Rejected").Count
                };

                // Donors by blood group
                _donorsByBloodGroup = new Dictionary<string, int>();
                string[] groups = { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" };
                foreach (var g in groups)
                    _donorsByBloodGroup[g] = _donorService.GetDonorsByBloodGroup(g).Count;

                // Refresh all chart panels
                pnlInventoryChart.Invalidate();
                pnlRequestChart.Invalidate();
                pnlDonorChart.Invalidate();

                lblStatus.Text = "✅  Charts updated: " + DateTime.Now.ToString("hh:mm:ss tt");
                lblStatus.ForeColor = Color.FromArgb(25, 135, 84);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌  Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        // ─────────────────────────────────────────
        // CHART 1: Blood Inventory — PIE CHART
        // ─────────────────────────────────────────
        private void DrawInventoryPieChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            var panel = (Panel)sender;

            // Title
            g.DrawString("Blood Units Available by Blood Group",
                new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(Color.FromArgb(30, 30, 40)),
                new PointF(20, 15));

            if (_inventorySummary == null || _inventorySummary.Count == 0)
            {
                g.DrawString("No inventory data available.", new Font("Segoe UI", 11),
                    Brushes.Gray, new PointF(panel.Width / 2 - 100, panel.Height / 2));
                return;
            }

            int total = 0;
            foreach (var v in _inventorySummary.Values) total += v;
            if (total == 0) { g.DrawString("All blood groups are empty.", new Font("Segoe UI", 11), Brushes.Gray, new PointF(200, 200)); return; }

            // Pie
            int cx = 280, cy = panel.Height / 2 - 20, r = Math.Min(200, panel.Height / 2 - 60);
            var rect = new Rectangle(cx - r, cy - r, r * 2, r * 2);
            float startAngle = -90f;
            int ci = 0;
            var keys = new List<string>(_inventorySummary.Keys);

            foreach (var kv in _inventorySummary)
            {
                float sweep = (float)kv.Value / total * 360f;
                Color col = ChartColors[ci % ChartColors.Length];

                // Slightly explode the biggest slice
                int ex = 0, ey = 0;
                if (kv.Value == GetMax(_inventorySummary)) { ex = (int)(10 * Math.Cos(Math.PI / 180 * (startAngle + sweep / 2))); ey = (int)(10 * Math.Sin(Math.PI / 180 * (startAngle + sweep / 2))); }

                var pieRect = new Rectangle(rect.X + ex, rect.Y + ey, rect.Width, rect.Height);
                g.FillPie(new SolidBrush(col), pieRect, startAngle, sweep);
                g.DrawPie(new Pen(Color.White, 2), pieRect, startAngle, sweep);

                // Percentage label inside slice
                if (sweep > 15)
                {
                    float midAngle = startAngle + sweep / 2;
                    float lx = cx + ex + (r * 0.6f) * (float)Math.Cos(Math.PI / 180 * midAngle);
                    float ly = cy + ey + (r * 0.6f) * (float)Math.Sin(Math.PI / 180 * midAngle);
                    string pct = $"{(float)kv.Value / total * 100:0}%";
                    var sz = g.MeasureString(pct, new Font("Segoe UI", 9, FontStyle.Bold));
                    g.DrawString(pct, new Font("Segoe UI", 9, FontStyle.Bold),
                        Brushes.White, lx - sz.Width / 2, ly - sz.Height / 2);
                }

                startAngle += sweep;
                ci++;
            }

            // Legend
            int lx2 = cx + r + 40, ly2 = cy - r;
            ci = 0;
            foreach (var kv in _inventorySummary)
            {
                Color col = ChartColors[ci % ChartColors.Length];
                g.FillRectangle(new SolidBrush(col), lx2, ly2 + ci * 30, 18, 18);
                g.DrawString($"{kv.Key}  —  {kv.Value} units",
                    new Font("Segoe UI", 10), new SolidBrush(Color.FromArgb(40, 40, 40)),
                    lx2 + 24, ly2 + ci * 30);
                ci++;
            }

            // Total
            g.DrawString($"Total: {total} units", new Font("Segoe UI", 11, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(180, 0, 0)), lx2, ly2 + ci * 30 + 10);
        }

        // ─────────────────────────────────────────
        // CHART 2: Request Status — BAR CHART
        // ─────────────────────────────────────────
        private void DrawRequestBarChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            var panel = (Panel)sender;

            g.DrawString("Blood Requests by Status",
                new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(Color.FromArgb(30, 30, 40)),
                new PointF(20, 15));

            if (_requestStatus == null) return;

            int maxVal = 1;
            foreach (var v in _requestStatus.Values) if (v > maxVal) maxVal = v;

            int chartX = 80, chartY = 60;
            int chartW = panel.Width - 200;
            int chartH = panel.Height - 160;
            int barCount = _requestStatus.Count;
            int barW = Math.Min(100, chartW / barCount - 30);

            // Grid lines
            var gridPen = new Pen(Color.FromArgb(220, 220, 220), 1);
            for (int i = 0; i <= 5; i++)
            {
                int gy = chartY + chartH - (int)((float)i / 5 * chartH);
                g.DrawLine(gridPen, chartX, gy, chartX + chartW, gy);
                int val = (int)Math.Round((float)maxVal * i / 5);
                g.DrawString(val.ToString(), new Font("Segoe UI", 8),
                    Brushes.Gray, chartX - 35, gy - 8);
            }

            // Axes
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80), 2), chartX, chartY, chartX, chartY + chartH);
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80), 2), chartX, chartY + chartH, chartX + chartW, chartY + chartH);

            // Bars
            Color[] barColors = {
                Color.FromArgb(255, 153, 0),
                Color.FromArgb(25, 135, 84),
                Color.FromArgb(220, 53, 69)
            };

            int bi = 0;
            int spacing = chartW / (barCount + 1);
            foreach (var kv in _requestStatus)
            {
                int bh = maxVal == 0 ? 0 : (int)((float)kv.Value / maxVal * chartH);
                int bx = chartX + spacing * (bi + 1) - barW / 2;
                int by = chartY + chartH - bh;

                // Bar with gradient
                var rect = new Rectangle(bx, by, barW, bh);
                if (bh > 0)
                {
                    using var brush = new LinearGradientBrush(
                        new Point(bx, by), new Point(bx, by + bh),
                        barColors[bi % barColors.Length],
                        ControlPaint.Dark(barColors[bi % barColors.Length], 0.2f));
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(new Pen(Color.White, 1), rect);
                }

                // Value on top
                g.DrawString(kv.Value.ToString(), new Font("Segoe UI", 11, FontStyle.Bold),
                    new SolidBrush(barColors[bi % barColors.Length]),
                    bx + barW / 2 - 8, by - 22);

                // Label below
                var lbl = g.MeasureString(kv.Key, new Font("Segoe UI", 10));
                g.DrawString(kv.Key, new Font("Segoe UI", 10, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(40, 40, 40)),
                    bx + barW / 2 - lbl.Width / 2, chartY + chartH + 8);

                bi++;
            }

            // Y-axis label
            var state = g.Save();
            g.TranslateTransform(18, chartY + chartH / 2);
            g.RotateTransform(-90);
            g.DrawString("Number of Requests", new Font("Segoe UI", 9), Brushes.Gray, -60, -8);
            g.Restore(state);
        }

        // ─────────────────────────────────────────
        // CHART 3: Donors by Blood Group — BAR CHART
        // ─────────────────────────────────────────
        private void DrawDonorBarChart(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            var panel = (Panel)sender;

            g.DrawString("Registered Donors by Blood Group",
                new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(Color.FromArgb(30, 30, 40)),
                new PointF(20, 15));

            if (_donorsByBloodGroup == null) return;

            int maxVal = 1;
            foreach (var v in _donorsByBloodGroup.Values) if (v > maxVal) maxVal = v;

            int chartX = 80, chartY = 60;
            int chartW = panel.Width - 120;
            int chartH = panel.Height - 160;
            int barCount = _donorsByBloodGroup.Count;
            int barW = Math.Min(70, chartW / barCount - 15);

            // Grid lines
            var gridPen = new Pen(Color.FromArgb(220, 220, 220), 1);
            for (int i = 0; i <= 5; i++)
            {
                int gy = chartY + chartH - (int)((float)i / 5 * chartH);
                g.DrawLine(gridPen, chartX, gy, chartX + chartW, gy);
                int val = (int)Math.Round((float)maxVal * i / 5);
                g.DrawString(val.ToString(), new Font("Segoe UI", 8), Brushes.Gray, chartX - 30, gy - 8);
            }

            // Axes
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80), 2), chartX, chartY, chartX, chartY + chartH);
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80), 2), chartX, chartY + chartH, chartX + chartW, chartY + chartH);

            int bi = 0;
            int spacing = chartW / (barCount + 1);
            foreach (var kv in _donorsByBloodGroup)
            {
                int bh = maxVal == 0 ? 0 : (int)((float)kv.Value / maxVal * chartH);
                int bx = chartX + spacing * (bi + 1) - barW / 2;
                int by = chartY + chartH - bh;
                Color col = ChartColors[bi % ChartColors.Length];

                if (bh > 0)
                {
                    using var brush = new LinearGradientBrush(
                        new Point(bx, by), new Point(bx, by + bh),
                        col, ControlPaint.Dark(col, 0.2f));
                    g.FillRectangle(brush, new Rectangle(bx, by, barW, bh));
                    g.DrawRectangle(new Pen(Color.White, 1), new Rectangle(bx, by, barW, bh));
                }

                // Value on top
                var valStr = kv.Value.ToString();
                var sz = g.MeasureString(valStr, new Font("Segoe UI", 10, FontStyle.Bold));
                g.DrawString(valStr, new Font("Segoe UI", 10, FontStyle.Bold),
                    new SolidBrush(col), bx + barW / 2 - sz.Width / 2, by - 22);

                // Blood group label
                var lsz = g.MeasureString(kv.Key, new Font("Segoe UI", 9, FontStyle.Bold));
                g.DrawString(kv.Key, new Font("Segoe UI", 9, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(40, 40, 40)),
                    bx + barW / 2 - lsz.Width / 2, chartY + chartH + 8);

                bi++;
            }

            // Y-axis label
            var state = g.Save();
            g.TranslateTransform(18, chartY + chartH / 2);
            g.RotateTransform(-90);
            g.DrawString("Number of Donors", new Font("Segoe UI", 9), Brushes.Gray, -55, -8);
            g.Restore(state);
        }

        private int GetMax(Dictionary<string, int> d)
        {
            int max = 0;
            foreach (var v in d.Values) if (v > max) max = v;
            return max;
        }

        private Button MakeBtn(string text, Color color, Point loc, Size size)
        {
            var b = new Button { Text = text, Location = loc, Size = size, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
