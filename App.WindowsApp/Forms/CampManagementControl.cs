using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class CampManagementControl : UserControl
    {
        private DataGridView dgvCamps;
        private DataGridView dgvCampVolunteers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnSearch, btnAssignVolunteers;
        private Label lblTitle, lblCount, lblVolunteersHeader, lblStatusBar;
        private Panel detailPanel;
        private bool _sortAscending = true;
        private string _lastSortColumn = "";
        private CampService campService = new CampService();

        public CampManagementControl()
        {
            InitializeComponents();
            LoadCamps();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);

            lblTitle = new Label
            {
                Text = "⛺  NGO Blood Donation Camps",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var toolbar = new Panel
            {
                Location = new Point(0, 45),
                Size = new Size(985, 55),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            txtSearch = new TextBox
            {
                Size = new Size(300, 30),
                Location = new Point(10, 12),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSearch = CreateButton("🔍 Search", Color.FromArgb(52, 152, 219), new Point(320, 12), new Size(100, 30));
            btnSearch.Click += (s, e) => SearchCamps();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchCamps(); };

            btnAdd = CreateButton("➕ Add Camp", Color.FromArgb(46, 204, 113), new Point(620, 12), new Size(110, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(740, 12), new Size(70, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(231, 76, 60), new Point(820, 12), new Size(80, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(910, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadCamps(); };

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh });

            // Grid for Camps (Left part)
            dgvCamps = new DataGridView
            {
                Location = new Point(0, 110),
                Size = new Size(620, 430),
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
            dgvCamps.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvCamps.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            dgvCamps.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCamps.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(180, 0, 0);
            dgvCamps.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            dgvCamps.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            dgvCamps.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCamps.EnableHeadersVisualStyles = false;
            dgvCamps.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);
            dgvCamps.SelectionChanged += DgvCamps_SelectionChanged;
            dgvCamps.ColumnHeaderMouseClick += DgvCamps_ColumnHeaderMouseClick;

            // Details/Volunteers panel (Right part)
            detailPanel = new Panel
            {
                Location = new Point(635, 110),
                Size = new Size(350, 430),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblVolunteersHeader = new Label
            {
                Text = "Camp Volunteers",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Location = new Point(10, 10),
                Size = new Size(330, 25)
            };

            dgvCampVolunteers = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(328, 330),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9),
                ColumnHeadersHeight = 30,
                RowTemplate = { Height = 28 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCampVolunteers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvCampVolunteers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(22, 27, 38);
            dgvCampVolunteers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCampVolunteers.EnableHeadersVisualStyles = false;

            btnAssignVolunteers = CreateButton("🤝 Assign / Manage Volunteers", Color.FromArgb(25, 135, 84), new Point(10, 385), new Size(328, 35));
            btnAssignVolunteers.Click += BtnAssignVolunteers_Click;

            detailPanel.Controls.AddRange(new Control[] { lblVolunteersHeader, dgvCampVolunteers, btnAssignVolunteers });

            lblCount = new Label
            {
                Text = "Total: 0 camps",
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

            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvCamps, detailPanel, lblCount, lblStatusBar });
        }

        private void DgvCamps_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = dgvCamps.Columns[e.ColumnIndex].HeaderText;

            if (_lastSortColumn == colName)
                _sortAscending = !_sortAscending;
            else
            {
                _lastSortColumn = colName;
                _sortAscending = true;
            }

            if (dgvCamps.DataSource is System.Data.DataTable dt)
            {
                dt.DefaultView.Sort = $"[{colName}] {(_sortAscending ? "ASC" : "DESC")}";
                dgvCamps.DataSource = dt.DefaultView.ToTable();

                if (dgvCamps.Columns.Contains("CampID"))
                    dgvCamps.Columns["CampID"].Visible = false;

                lblStatusBar.Text = $"  At Sorted by {colName} ({(_sortAscending ? "A → Z" : "Z → A")})";
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

        private void LoadCamps()
        {
            try
            {
                lblStatusBar.Text = "  ⏳ Loading camps...";
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = false;

                List<Camp> camps = campService.GetAllCamps();

                dgvCamps.Columns.Clear();
                dgvCamps.DataSource = null;

                var dt = new System.Data.DataTable();
                dt.Columns.Add("CampID", typeof(int));
                dt.Columns.Add("Camp Name");
                dt.Columns.Add("Venue / Location");
                dt.Columns.Add("Date");
                dt.Columns.Add("Target Units");
                dt.Columns.Add("Organizer");

                foreach (var c in camps)
                    dt.Rows.Add(c.CampID, c.CampName, c.Location, c.CampDate.ToString("dd-MMM-yyyy"), c.TargetUnits, c.OrganizerName);

                dgvCamps.DataSource = dt;
                dgvCamps.Columns["CampID"].Visible = false;

                lblCount.Text = $"Total: {camps.Count} camp(s)";
                lblStatusBar.Text = $"  ✅ {camps.Count} camp(s) loaded  |  {DateTime.Now:hh:mm:ss tt}";
                
                if (camps.Count > 0)
                {
                    dgvCamps.Rows[0].Selected = true;
                    LoadCampVolunteers();
                }
                else
                {
                    dgvCampVolunteers.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading camps: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatusBar.Text = "  ❌ Error loading data";
            }
            finally
            {
                btnAdd.Enabled = btnEdit.Enabled = btnDelete.Enabled = true;
            }
        }

        private void SearchCamps()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) { LoadCamps(); return; }

                string keyword = txtSearch.Text.Trim();
                var camps = campService.SearchCamps(keyword);

                dgvCamps.DataSource = null;
                var dt = new System.Data.DataTable();
                dt.Columns.Add("CampID", typeof(int));
                dt.Columns.Add("Camp Name");
                dt.Columns.Add("Venue / Location");
                dt.Columns.Add("Date");
                dt.Columns.Add("Target Units");
                dt.Columns.Add("Organizer");

                foreach (var c in camps)
                    dt.Rows.Add(c.CampID, c.CampName, c.Location, c.CampDate.ToString("dd-MMM-yyyy"), c.TargetUnits, c.OrganizerName);

                dgvCamps.DataSource = dt;
                dgvCamps.Columns["CampID"].Visible = false;
                lblCount.Text = $"Found: {camps.Count} camp(s)";
                lblStatusBar.Text = $"  🔍 '{txtSearch.Text}' — {camps.Count} result(s)";

                if (camps.Count > 0)
                {
                    dgvCamps.Rows[0].Selected = true;
                    LoadCampVolunteers();
                }
                else
                {
                    dgvCampVolunteers.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCampVolunteers()
        {
            if (dgvCamps.SelectedRows.Count == 0)
            {
                dgvCampVolunteers.DataSource = null;
                lblVolunteersHeader.Text = "Camp Volunteers";
                btnAssignVolunteers.Enabled = false;
                return;
            }

            try
            {
                int campId = Convert.ToInt32(dgvCamps.SelectedRows[0].Cells["CampID"].Value);
                string campName = dgvCamps.SelectedRows[0].Cells["Camp Name"].Value.ToString() ?? "";
                
                lblVolunteersHeader.Text = campName.Length > 25 ? campName.Substring(0, 22) + "..." : campName;
                btnAssignVolunteers.Enabled = true;

                var volunteers = campService.GetVolunteersForCamp(campId);

                dgvCampVolunteers.DataSource = null;
                var dt = new System.Data.DataTable();
                dt.Columns.Add("VolunteerID", typeof(int));
                dt.Columns.Add("Name");
                dt.Columns.Add("Phone");
                dt.Columns.Add("Skills");

                foreach (var v in volunteers)
                    dt.Rows.Add(v.VolunteerID, v.FullName, v.PhoneNumber, v.Skills);

                dgvCampVolunteers.DataSource = dt;
                dgvCampVolunteers.Columns["VolunteerID"].Visible = false;
            }
            catch (Exception ex)
            {
                lblStatusBar.Text = "Error loading camp volunteers: " + ex.Message;
            }
        }

        private void DgvCamps_SelectionChanged(object sender, EventArgs e)
        {
            LoadCampVolunteers();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new CampForm(null);
            if (form.ShowDialog() == DialogResult.OK) { LoadCamps(); lblStatusBar.Text = "  ✅ New camp added"; }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCamps.SelectedRows.Count == 0) { MessageBox.Show("Please select a camp to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int campId = Convert.ToInt32(dgvCamps.SelectedRows[0].Cells["CampID"].Value);
            Camp camp = campService.GetCampById(campId);
            if (camp == null) return;
            var form = new CampForm(camp);
            if (form.ShowDialog() == DialogResult.OK) { LoadCamps(); lblStatusBar.Text = "  ✅ Camp details updated"; }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCamps.SelectedRows.Count == 0) { MessageBox.Show("Please select a camp to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = dgvCamps.SelectedRows[0].Cells["Camp Name"].Value.ToString();
            if (MessageBox.Show($"Delete blood camp '{name}'?\n\nThis will soft-delete the camp.", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int campId = Convert.ToInt32(dgvCamps.SelectedRows[0].Cells["CampID"].Value);
                if (campService.DeleteCamp(campId))
                { MessageBox.Show("Camp deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadCamps(); lblStatusBar.Text = $"  🗑️ '{name}' deleted"; }
                else
                    MessageBox.Show("Failed to delete camp.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAssignVolunteers_Click(object sender, EventArgs e)
        {
            if (dgvCamps.SelectedRows.Count == 0) return;
            int campId = Convert.ToInt32(dgvCamps.SelectedRows[0].Cells["CampID"].Value);
            string name = dgvCamps.SelectedRows[0].Cells["Camp Name"].Value.ToString() ?? "";

            var form = new AssignVolunteersForm(campId, name);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadCampVolunteers();
                lblStatusBar.Text = "  ✅ Camp volunteers updated";
            }
        }
    }
}
