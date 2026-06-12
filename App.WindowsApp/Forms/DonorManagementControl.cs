using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class DonorManagementControl : UserControl
    {
        private DataGridView dgvDonors;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnSearch;
        private ComboBox cboBloodGroupFilter;
        private Label lblTitle, lblCount, lblStatusBar;
        private bool _sortAscending = true;
        private string _lastSortColumn = "";
        private DonorService donorService = new DonorService();

        public DonorManagementControl()
        {
            InitializeComponents();
            LoadDonors();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);

            lblTitle = new Label
            {
                Text = "🩸  Donor Management",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var toolbar = new Panel
            {
                Location = new Point(0, 45),
                Size = new Size(960, 55),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            txtSearch = new TextBox
            {
                Size = new Size(280, 30),
                Location = new Point(10, 12),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSearch = CreateButton("🔍 Search", Color.FromArgb(52, 152, 219), new Point(300, 12), new Size(100, 30));
            btnSearch.Click += (s, e) => SearchDonors();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchDonors(); };

            var lblFilter = new Label { Text = "Blood Group:", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(415, 18), ForeColor = Color.Gray };
            cboBloodGroupFilter = new ComboBox
            {
                Location = new Point(500, 13),
                Size = new Size(90, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboBloodGroupFilter.Items.AddRange(new object[] { "All", "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });
            cboBloodGroupFilter.SelectedIndex = 0;
            cboBloodGroupFilter.SelectedIndexChanged += (s, e) => LoadDonors();

            btnAdd = CreateButton("➕ Add Donor", Color.FromArgb(46, 204, 113), new Point(610, 12), new Size(120, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(740, 12), new Size(90, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(231, 76, 60), new Point(840, 12), new Size(90, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(940, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cboBloodGroupFilter.SelectedIndex = 0; LoadDonors(); };

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, lblFilter, cboBloodGroupFilter, btnAdd, btnEdit, btnDelete, btnRefresh });

            dgvDonors = new DataGridView
            {
                Location = new Point(0, 110),
                Size = new Size(985, 430),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvDonors.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvDonors.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            dgvDonors.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDonors.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 0, 0);
            dgvDonors.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            dgvDonors.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            dgvDonors.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvDonors.EnableHeadersVisualStyles = false;
            dgvDonors.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);
            dgvDonors.ColumnHeaderMouseClick += DgvDonors_ColumnHeaderMouseClick;

            lblCount = new Label
            {
                Text = "Total: 0 donors",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 548)
            };

            lblStatusBar = new Label
            {
                Text = "  Ready",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvDonors, lblCount, lblStatusBar });
        }

        private void DgvDonors_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = dgvDonors.Columns[e.ColumnIndex].HeaderText;

            if (_lastSortColumn == colName)
                _sortAscending = !_sortAscending;
            else
            {
                _lastSortColumn = colName;
                _sortAscending = true;
            }

            if (dgvDonors.DataSource is System.Data.DataTable dt)
            {
                dt.DefaultView.Sort = $"[{colName}] {(_sortAscending ? "ASC" : "DESC")}";
                dgvDonors.DataSource = dt.DefaultView.ToTable();

                if (dgvDonors.Columns.Contains("DonorID"))
                    dgvDonors.Columns["DonorID"].Visible = false;

                if (dgvDonors.Columns.Contains("Blood Group"))
                {
                    dgvDonors.Columns["Blood Group"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                    dgvDonors.Columns["Blood Group"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }

                lblStatusBar.Text = $"  🔃 Sorted by {colName} ({(_sortAscending ? "A → Z" : "Z → A")})";
            }
        }

        private Button CreateButton(string text, Color backColor, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadDonors()
        {
            try
            {
                lblStatusBar.Text = "  ⏳ Loading donors...";
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = false;

                List<Donor> donors;
                if (cboBloodGroupFilter.SelectedItem?.ToString() == "All" || cboBloodGroupFilter.SelectedIndex == 0)
                    donors = donorService.GetAllDonors();
                else
                    donors = donorService.GetDonorsByBloodGroup(cboBloodGroupFilter.SelectedItem.ToString());

                dgvDonors.Columns.Clear();
                dgvDonors.DataSource = null;

                var dt = new System.Data.DataTable();
                dt.Columns.Add("DonorID", typeof(int));
                dt.Columns.Add("Full Name");
                dt.Columns.Add("CNIC");
                dt.Columns.Add("Blood Group");
                dt.Columns.Add("Age");
                dt.Columns.Add("Gender");
                dt.Columns.Add("Phone");
                dt.Columns.Add("Last Donation");

                foreach (var d in donors)
                    dt.Rows.Add(d.DonorID, d.FullName, d.CNIC, d.BloodGroup, d.Age,
                        d.Gender, d.PhoneNumber,
                        d.LastDonationDate.HasValue ? d.LastDonationDate.Value.ToString("dd-MMM-yyyy") : "Never");

                dgvDonors.DataSource = dt;
                dgvDonors.Columns["DonorID"].Visible = false;
                dgvDonors.Columns["Blood Group"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                dgvDonors.Columns["Blood Group"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                lblCount.Text = $"Total: {donors.Count} donor(s)";
                lblStatusBar.Text = $"  ✅ {donors.Count} donor(s) loaded  |  {DateTime.Now:hh:mm:ss tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading donors: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBar.Text = "  ❌ Error loading data";
            }
            finally
            {
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = true;
            }
        }

        private void SearchDonors()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) { LoadDonors(); return; }

                string keyword = txtSearch.Text.Trim().ToLower();
                var allDonors = donorService.GetAllDonors();
                var donors = allDonors.FindAll(d =>
                    d.FullName != null && d.FullName.ToLower().Contains(keyword));

                dgvDonors.DataSource = null;
                var dt = new System.Data.DataTable();
                dt.Columns.Add("DonorID", typeof(int));
                dt.Columns.Add("Full Name");
                dt.Columns.Add("CNIC");
                dt.Columns.Add("Blood Group");
                dt.Columns.Add("Age");
                dt.Columns.Add("Gender");
                dt.Columns.Add("Phone");
                dt.Columns.Add("Last Donation");

                foreach (var d in donors)
                    dt.Rows.Add(d.DonorID, d.FullName, d.CNIC, d.BloodGroup, d.Age, d.Gender, d.PhoneNumber,
                        d.LastDonationDate.HasValue ? d.LastDonationDate.Value.ToString("dd-MMM-yyyy") : "Never");

                dgvDonors.DataSource = dt;
                dgvDonors.Columns["DonorID"].Visible = false;
                dgvDonors.Columns["Blood Group"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                dgvDonors.Columns["Blood Group"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblCount.Text = $"Found: {donors.Count} donor(s)";
                lblStatusBar.Text = $"  🔍 '{txtSearch.Text}' — {donors.Count} result(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new DonorForm(null);
            if (form.ShowDialog() == DialogResult.OK) { LoadDonors(); lblStatusBar.Text = "  ✅ New donor added"; }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvDonors.SelectedRows.Count == 0) { MessageBox.Show("Please select a donor to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int donorId = Convert.ToInt32(dgvDonors.SelectedRows[0].Cells["DonorID"].Value);
            Donor donor = donorService.GetDonorById(donorId);
            if (donor == null) return;
            var form = new DonorForm(donor);
            if (form.ShowDialog() == DialogResult.OK) { LoadDonors(); lblStatusBar.Text = "  ✅ Donor updated"; }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDonors.SelectedRows.Count == 0) { MessageBox.Show("Please select a donor to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = dgvDonors.SelectedRows[0].Cells["Full Name"].Value.ToString();
            if (MessageBox.Show($"Delete donor '{name}'?\n\nThis will mark them as inactive.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int donorId = Convert.ToInt32(dgvDonors.SelectedRows[0].Cells["DonorID"].Value);
                if (donorService.DeleteDonor(donorId))
                { MessageBox.Show("Donor deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadDonors(); lblStatusBar.Text = $"  🗑️ '{name}' deleted"; }
                else
                    MessageBox.Show("Failed to delete donor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}