using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class VolunteerManagementControl : UserControl
    {
        private DataGridView dgvVolunteers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnSearch;
        private Label lblTitle, lblCount, lblStatusBar;
        private bool _sortAscending = true;
        private string _lastSortColumn = "";
        private VolunteerService volunteerService = new VolunteerService();

        public VolunteerManagementControl()
        {
            InitializeComponents();
            LoadVolunteers();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);

            lblTitle = new Label
            {
                Text = "🤝  Volunteer Management",
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
                Size = new Size(320, 30),
                Location = new Point(10, 12),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSearch = CreateButton("🔍 Search", Color.FromArgb(52, 152, 219), new Point(340, 12), new Size(100, 30));
            btnSearch.Click += (s, e) => SearchVolunteers();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchVolunteers(); };

            btnAdd = CreateButton("➕ Add Volunteer", Color.FromArgb(46, 204, 113), new Point(610, 12), new Size(130, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(750, 12), new Size(80, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(231, 76, 60), new Point(840, 12), new Size(80, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(930, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadVolunteers(); };

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh });

            dgvVolunteers = new DataGridView
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
            dgvVolunteers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvVolunteers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(22, 27, 38);
            dgvVolunteers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVolunteers.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(22, 27, 38);
            dgvVolunteers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvVolunteers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(46, 204, 113);
            dgvVolunteers.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvVolunteers.EnableHeadersVisualStyles = false;
            dgvVolunteers.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);
            dgvVolunteers.ColumnHeaderMouseClick += DgvVolunteers_ColumnHeaderMouseClick;

            lblCount = new Label
            {
                Text = "Total: 0 volunteers",
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

            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvVolunteers, lblCount, lblStatusBar });
        }

        private void DgvVolunteers_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = dgvVolunteers.Columns[e.ColumnIndex].HeaderText;

            if (_lastSortColumn == colName)
                _sortAscending = !_sortAscending;
            else
            {
                _lastSortColumn = colName;
                _sortAscending = true;
            }

            if (dgvVolunteers.DataSource is System.Data.DataTable dt)
            {
                dt.DefaultView.Sort = $"[{colName}] {(_sortAscending ? "ASC" : "DESC")}";
                dgvVolunteers.DataSource = dt.DefaultView.ToTable();

                if (dgvVolunteers.Columns.Contains("VolunteerID"))
                    dgvVolunteers.Columns["VolunteerID"].Visible = false;

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

        private void LoadVolunteers()
        {
            try
            {
                lblStatusBar.Text = "  ⏳ Loading volunteers...";
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = false;

                List<Volunteer> volunteers = volunteerService.GetAllVolunteers();

                dgvVolunteers.Columns.Clear();
                dgvVolunteers.DataSource = null;

                var dt = new System.Data.DataTable();
                dt.Columns.Add("VolunteerID", typeof(int));
                dt.Columns.Add("Full Name");
                dt.Columns.Add("Phone");
                dt.Columns.Add("Email");
                dt.Columns.Add("Address");
                dt.Columns.Add("Skills");

                foreach (var v in volunteers)
                    dt.Rows.Add(v.VolunteerID, v.FullName, v.PhoneNumber, v.Email, v.Address, v.Skills);

                dgvVolunteers.DataSource = dt;
                dgvVolunteers.Columns["VolunteerID"].Visible = false;

                lblCount.Text = $"Total: {volunteers.Count} volunteer(s)";
                lblStatusBar.Text = $"  ✅ {volunteers.Count} volunteer(s) loaded  |  {DateTime.Now:hh:mm:ss tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBar.Text = "  ❌ Error loading data";
            }
            finally
            {
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = true;
            }
        }

        private void SearchVolunteers()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) { LoadVolunteers(); return; }

                string keyword = txtSearch.Text.Trim();
                var volunteers = volunteerService.SearchVolunteers(keyword);

                dgvVolunteers.DataSource = null;
                var dt = new System.Data.DataTable();
                dt.Columns.Add("VolunteerID", typeof(int));
                dt.Columns.Add("Full Name");
                dt.Columns.Add("Phone");
                dt.Columns.Add("Email");
                dt.Columns.Add("Address");
                dt.Columns.Add("Skills");

                foreach (var v in volunteers)
                    dt.Rows.Add(v.VolunteerID, v.FullName, v.PhoneNumber, v.Email, v.Address, v.Skills);

                dgvVolunteers.DataSource = dt;
                dgvVolunteers.Columns["VolunteerID"].Visible = false;
                lblCount.Text = $"Found: {volunteers.Count} volunteer(s)";
                lblStatusBar.Text = $"  🔍 '{txtSearch.Text}' — {volunteers.Count} result(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new VolunteerForm(null);
            if (form.ShowDialog() == DialogResult.OK) { LoadVolunteers(); lblStatusBar.Text = "  ✅ New volunteer added"; }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvVolunteers.SelectedRows.Count == 0) { MessageBox.Show("Please select a volunteer to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int volunteerId = Convert.ToInt32(dgvVolunteers.SelectedRows[0].Cells["VolunteerID"].Value);
            Volunteer volunteer = volunteerService.GetVolunteerById(volunteerId);
            if (volunteer == null) return;
            var form = new VolunteerForm(volunteer);
            if (form.ShowDialog() == DialogResult.OK) { LoadVolunteers(); lblStatusBar.Text = "  ✅ Volunteer updated"; }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvVolunteers.SelectedRows.Count == 0) { MessageBox.Show("Please select a volunteer to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = dgvVolunteers.SelectedRows[0].Cells["Full Name"].Value.ToString();
            if (MessageBox.Show($"Delete volunteer '{name}'?\n\nThis will remove their associations.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int volunteerId = Convert.ToInt32(dgvVolunteers.SelectedRows[0].Cells["VolunteerID"].Value);
                if (volunteerService.DeleteVolunteer(volunteerId))
                { MessageBox.Show("Volunteer deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadVolunteers(); lblStatusBar.Text = $"  🗑️ '{name}' deleted"; }
                else
                    MessageBox.Show("Failed to delete volunteer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
