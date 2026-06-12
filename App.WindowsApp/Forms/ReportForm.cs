using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class ReportForm : Form
    {
        private readonly DonorService donorService = new DonorService();
        private readonly BloodInventoryService inventoryService = new BloodInventoryService();
        private readonly RecipientService recipientService = new RecipientService();
        private readonly BloodRequestService requestService = new BloodRequestService();

        private TabControl tabControl;
        private DataGridView dgvDonors, dgvInventory, dgvRecipients, dgvRequests, dgvDonations;
        private Button btnPrint, btnRefresh, btnClose;
        private Label lblGenerated;
        private PrintDocument printDoc;
        private string printContent = "";

        public ReportForm()
        {
            InitializeComponents();
            LoadAllReports();
        }

        private void InitializeComponents()
        {
            this.Text = "Reports & Summary";
            this.Size = new Size(1050, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.MinimumSize = new Size(900, 600);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(180, 0, 0) };
            header.Controls.Add(new Label { Text = "📊  Reports & Analytics", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(14, 14) });

            lblGenerated = new Label { Text = "", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 200, 200), AutoSize = true, Location = new Point(700, 20) };
            header.Controls.Add(lblGenerated);

            // Tabs
            tabControl = new TabControl
            {
                Location = new Point(10, 65),
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            dgvDonors = MakeGrid();
            dgvInventory = MakeGrid();
            dgvRecipients = MakeGrid();
            dgvRequests = MakeGrid();
            dgvDonations = MakeGrid();

            tabControl.TabPages.Add(MakeTab("🩸  Donors", dgvDonors));
            tabControl.TabPages.Add(MakeTab("🏥  Blood Inventory", dgvInventory));
            tabControl.TabPages.Add(MakeTab("👥  Recipients", dgvRecipients));
            tabControl.TabPages.Add(MakeTab("📋  Blood Requests", dgvRequests));
            tabControl.TabPages.Add(MakeTab("💉  Donation History", dgvDonations));

            // Buttons
            var btnBar = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = Color.White };

            btnPrint = MakeBtn("🖨️  Print Report", Color.FromArgb(52, 152, 219), new Point(14, 10), new Size(160, 35));
            btnPrint.Click += BtnPrint_Click;

            btnRefresh = MakeBtn("🔄  Refresh", Color.FromArgb(46, 204, 113), new Point(185, 10), new Size(120, 35));
            btnRefresh.Click += (s, e) => LoadAllReports();

            btnClose = MakeBtn("✖  Close", Color.FromArgb(149, 165, 166), new Point(315, 10), new Size(100, 35));
            btnClose.Click += (s, e) => Close();

            var lblTip = new Label { Text = "Tip: Click a tab to view different reports. Use Print to export the current report.", Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = Color.Gray, AutoSize = true, Location = new Point(430, 18) };

            btnBar.Controls.AddRange(new Control[] { btnPrint, btnRefresh, btnClose, lblTip });

            this.Controls.Add(header);
            this.Controls.Add(tabControl);
            this.Controls.Add(btnBar);

            tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 130);
            this.Resize += (s, e) => tabControl.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 130);
        }

        private DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 28 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.EnableHeadersVisualStyles = false;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            return g;
        }

        private TabPage MakeTab(string title, DataGridView grid)
        {
            var tab = new TabPage(title) { BackColor = Color.White };
            tab.Controls.Add(grid);
            return tab;
        }

        private Button MakeBtn(string text, Color color, Point loc, Size size)
        {
            var b = new Button { Text = text, Location = loc, Size = size, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadAllReports()
        {
            try
            {
                LoadDonorsReport();
                LoadInventoryReport();
                LoadRecipientsReport();
                LoadRequestsReport();
                LoadDonationsReport();
                lblGenerated.Text = "Generated: " + DateTime.Now.ToString("dd-MMM-yyyy  hh:mm tt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reports:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDonorsReport()
        {
            var donors = donorService.GetAllDonors();
            var dt = new DataTable();
            dt.Columns.Add("#"); dt.Columns.Add("Full Name"); dt.Columns.Add("CNIC");
            dt.Columns.Add("Blood Group"); dt.Columns.Add("Age"); dt.Columns.Add("Gender");
            dt.Columns.Add("Phone"); dt.Columns.Add("Last Donation");

            int i = 1;
            foreach (var d in donors)
                dt.Rows.Add(i++, d.FullName, d.CNIC, d.BloodGroup, d.Age, d.Gender, d.PhoneNumber,
                    d.LastDonationDate.HasValue ? d.LastDonationDate.Value.ToString("dd-MMM-yyyy") : "Never");

            dgvDonors.DataSource = dt;
            StyleBloodGroupCol(dgvDonors, "Blood Group");
            tabControl.TabPages[0].Text = $"🩸  Donors ({donors.Count})";
        }

        private void LoadInventoryReport()
        {
            var list = inventoryService.GetAllInventory();
            var dt = new DataTable();
            dt.Columns.Add("#"); dt.Columns.Add("Blood Group"); dt.Columns.Add("Units");
            dt.Columns.Add("Collection Date"); dt.Columns.Add("Expiry Date"); dt.Columns.Add("Days Left");
            dt.Columns.Add("Donor"); dt.Columns.Add("Status");

            int i = 1;
            foreach (var inv in list)
                dt.Rows.Add(i++, inv.BloodGroup, inv.Units,
                    inv.CollectionDate.ToString("dd-MMM-yyyy"),
                    inv.ExpiryDate.ToString("dd-MMM-yyyy"),
                    inv.IsExpired ? "EXPIRED" : inv.DaysUntilExpiry + " days",
                    inv.DonorName, inv.Status);

            dgvInventory.DataSource = dt;
            StyleBloodGroupCol(dgvInventory, "Blood Group");

            // Color expired rows
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (row.Cells["Days Left"].Value?.ToString() == "EXPIRED")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
            }
            tabControl.TabPages[1].Text = $"🏥  Blood Inventory ({list.Count})";
        }

        private void LoadRecipientsReport()
        {
            var list = recipientService.GetAllRecipients();
            var dt = new DataTable();
            dt.Columns.Add("#"); dt.Columns.Add("Full Name"); dt.Columns.Add("CNIC");
            dt.Columns.Add("Blood Group"); dt.Columns.Add("Gender"); dt.Columns.Add("Phone");
            dt.Columns.Add("Hospital"); dt.Columns.Add("Medical Condition");

            int i = 1;
            foreach (var r in list)
                dt.Rows.Add(i++, r.FullName, r.CNIC, r.BloodGroup, r.Gender, r.PhoneNumber, r.HospitalName, r.MedicalCondition);

            dgvRecipients.DataSource = dt;
            StyleBloodGroupCol(dgvRecipients, "Blood Group");
            tabControl.TabPages[2].Text = $"👥  Recipients ({list.Count})";
        }

        private void LoadRequestsReport()
        {
            var list = requestService.GetAllRequests();
            var dt = new DataTable();
            dt.Columns.Add("#"); dt.Columns.Add("Recipient"); dt.Columns.Add("Blood Group");
            dt.Columns.Add("Units"); dt.Columns.Add("Urgency"); dt.Columns.Add("Request Date");
            dt.Columns.Add("Required By"); dt.Columns.Add("Status"); dt.Columns.Add("Approved By");

            int i = 1;
            foreach (var r in list)
                dt.Rows.Add(i++, r.RecipientName, r.BloodGroup, r.UnitsRequired, r.UrgencyLevel,
                    r.RequestDate.ToString("dd-MMM-yyyy"),
                    r.RequiredByDate.HasValue ? r.RequiredByDate.Value.ToString("dd-MMM-yyyy") : "—",
                    r.Status, r.ApprovedBy);

            dgvRequests.DataSource = dt;
            StyleBloodGroupCol(dgvRequests, "Blood Group");

            foreach (DataGridViewRow row in dgvRequests.Rows)
            {
                string status = row.Cells["Status"].Value?.ToString() ?? "";
                if (status == "Approved") row.Cells["Status"].Style.ForeColor = Color.FromArgb(39, 174, 96);
                else if (status == "Rejected") row.Cells["Status"].Style.ForeColor = Color.Red;
                else row.Cells["Status"].Style.ForeColor = Color.OrangeRed;
                row.Cells["Status"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
            tabControl.TabPages[3].Text = $"📋  Blood Requests ({list.Count})";
        }

        private void LoadDonationsReport()
        {
            try
            {
                string sql = @"SELECT d.DonationID, dn.FullName as Donor, d.BloodGroup, d.UnitsDonated,
                               d.DonationDate, d.BloodPressure, d.Hemoglobin, d.Notes
                               FROM Donations d
                               JOIN Donors dn ON d.DonorID = dn.DonorID
                               ORDER BY d.DonationDate DESC";
                DataTable raw = DatabaseHelper.ExecuteQuery(sql);

                var dt = new DataTable();
                dt.Columns.Add("#"); dt.Columns.Add("Donor Name"); dt.Columns.Add("Blood Group");
                dt.Columns.Add("Units"); dt.Columns.Add("Date"); dt.Columns.Add("Blood Pressure");
                dt.Columns.Add("Hemoglobin"); dt.Columns.Add("Notes");

                int i = 1;
                foreach (DataRow row in raw.Rows)
                    dt.Rows.Add(i++, row["Donor"].ToString(), row["BloodGroup"].ToString(),
                        row["UnitsDonated"].ToString(), Convert.ToDateTime(row["DonationDate"]).ToString("dd-MMM-yyyy"),
                        row["BloodPressure"] == DBNull.Value ? "—" : row["BloodPressure"].ToString(),
                        row["Hemoglobin"] == DBNull.Value ? "—" : row["Hemoglobin"].ToString(),
                        row["Notes"] == DBNull.Value ? "" : row["Notes"].ToString());

                dgvDonations.DataSource = dt;
                StyleBloodGroupCol(dgvDonations, "Blood Group");
                tabControl.TabPages[4].Text = $"💉  Donation History ({raw.Rows.Count})";
            }
            catch
            {
                // Donations table might be empty
                tabControl.TabPages[4].Text = "💉  Donation History (0)";
            }
        }

        private void StyleBloodGroupCol(DataGridView grid, string colName)
        {
            if (grid.Columns.Contains(colName))
            {
                grid.Columns[colName].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                grid.Columns[colName].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            // Build text content for printing
            string reportName = tabControl.SelectedTab?.Text ?? "Report";
            printContent = $"BLOOD BANK MANAGEMENT SYSTEM\r\n";
            printContent += $"Report: {reportName.Trim()}\r\n";
            printContent += $"Generated: {DateTime.Now:dd-MMM-yyyy hh:mm tt}\r\n";
            printContent += new string('=', 80) + "\r\n\r\n";

            DataGridView currentGrid = GetCurrentGrid();
            if (currentGrid?.DataSource is DataTable dt)
            {
                // Column headers
                foreach (DataGridViewColumn col in currentGrid.Columns)
                    if (col.Visible)
                        printContent += col.HeaderText.PadRight(20);
                printContent += "\r\n" + new string('-', 80) + "\r\n";

                // Rows
                foreach (DataGridViewRow row in currentGrid.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                        if (currentGrid.Columns[cell.ColumnIndex].Visible)
                            printContent += (cell.Value?.ToString() ?? "").PadRight(20);
                    printContent += "\r\n";
                }
            }
            printContent += "\r\n\r\n" + new string('=', 80) + "\r\n";
            printContent += $"Total records printed on {DateTime.Now:dd-MMM-yyyy}";

            printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDoc_PrintPage;

            var preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 900,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent
            };
            preview.ShowDialog();
        }

        int printLine = 0;
        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font f = new Font("Courier New", 9);
            float lineH = f.GetHeight(e.Graphics) + 2;
            string[] lines = printContent.Split(new[] { "\r\n" }, StringSplitOptions.None);
            float y = e.MarginBounds.Top;

            while (printLine < lines.Length)
            {
                if (y + lineH > e.MarginBounds.Bottom) { e.HasMorePages = true; break; }
                e.Graphics.DrawString(lines[printLine], f, Brushes.Black, e.MarginBounds.Left, y);
                y += lineH;
                printLine++;
            }
            if (printLine >= lines.Length) { printLine = 0; e.HasMorePages = false; }
        }

        private DataGridView GetCurrentGrid()
        {
            switch (tabControl.SelectedIndex)
            {
                case 0: return dgvDonors;
                case 1: return dgvInventory;
                case 2: return dgvRecipients;
                case 3: return dgvRequests;
                case 4: return dgvDonations;
                default: return null;
            }
        }
    }
}
