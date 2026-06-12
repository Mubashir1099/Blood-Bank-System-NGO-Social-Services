using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class BloodInventoryControl : UserControl
    {
        private DataGridView dgvInventory;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private ComboBox cboBloodGroupFilter;
        private Label lblTitle, lblCount, lblStatusBar;
        private bool _sortAsc = true; private string _lastSort = "";
        private BloodInventoryService inventoryService = new BloodInventoryService();

        public BloodInventoryControl()
        {
            InitializeComponents();
            LoadInventory();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);

            lblTitle = new Label
            {
                Text = "🏥  Blood Inventory Management",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(0, 5)
            };

            var toolbar = new Panel { Location = new Point(0, 45), Size = new Size(960, 55), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            var lblFilter = new Label { Text = "Filter Blood Group:", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(10, 18), ForeColor = Color.Gray };
            cboBloodGroupFilter = new ComboBox { Location = new Point(130, 13), Size = new Size(100, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBloodGroupFilter.Items.AddRange(new object[] { "All", "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });
            cboBloodGroupFilter.SelectedIndex = 0;
            cboBloodGroupFilter.SelectedIndexChanged += (s, e) => LoadInventory();

            btnAdd = CreateButton("➕ Add Stock", Color.FromArgb(46, 204, 113), new Point(620, 12), new Size(120, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(750, 12), new Size(90, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(231, 76, 60), new Point(850, 12), new Size(90, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(950, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { cboBloodGroupFilter.SelectedIndex = 0; LoadInventory(); };

            toolbar.Controls.AddRange(new Control[] { lblFilter, cboBloodGroupFilter, btnAdd, btnEdit, btnDelete, btnRefresh });

            dgvInventory = new DataGridView
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
            dgvInventory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            dgvInventory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvInventory.EnableHeadersVisualStyles = false;
            dgvInventory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            dgvInventory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            dgvInventory.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvInventory.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);

            lblCount = new Label { Text = "Total: 0 records", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 548) };

            lblStatusBar = new Label { Text = "  Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.White, BackColor = Color.FromArgb(40,40,40), Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,0,0) };
            dgvInventory.ColumnHeaderMouseClick += (s,e) => {
                string col = dgvInventory.Columns[e.ColumnIndex].HeaderText;
                if (_lastSort == col) _sortAsc = !_sortAsc; else { _lastSort = col; _sortAsc = true; }
                if (dgvInventory.DataSource is System.Data.DataTable dt2) { dt2.DefaultView.Sort = $"[{col}] {(_sortAsc?"ASC":"DESC")}"; dgvInventory.DataSource = dt2.DefaultView.ToTable(); }
                lblStatusBar.Text = $"  🔃 Sorted by {col}";
            };
            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvInventory, lblCount, lblStatusBar });
        }

        private Button CreateButton(string text, Color backColor, Point location, Size size)
        {
            var btn = new Button { Text = text, Location = location, Size = size, BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadInventory()
        {
            try
            {
                var inventories = cboBloodGroupFilter.SelectedItem?.ToString() == "All" || cboBloodGroupFilter.SelectedIndex == 0
                    ? inventoryService.GetAllInventory()
                    : inventoryService.GetInventoryByBloodGroup(cboBloodGroupFilter.SelectedItem.ToString());

                var dt = new System.Data.DataTable();
                dt.Columns.Add("InventoryID", typeof(int));
                dt.Columns.Add("Blood Group");
                dt.Columns.Add("Units");
                dt.Columns.Add("Collection Date");
                dt.Columns.Add("Expiry Date");
                dt.Columns.Add("Days Left");
                dt.Columns.Add("Donor Name");
                dt.Columns.Add("Status");

                foreach (var inv in inventories)
                    dt.Rows.Add(inv.InventoryID, inv.BloodGroup, inv.Units,
                        inv.CollectionDate.ToString("dd-MMM-yyyy"),
                        inv.ExpiryDate.ToString("dd-MMM-yyyy"),
                        inv.IsExpired ? "EXPIRED" : inv.DaysUntilExpiry.ToString() + " days",
                        inv.DonorName, inv.Status);

                dgvInventory.DataSource = dt;
                dgvInventory.Columns["InventoryID"].Visible = false;

                // Color code rows
                foreach (DataGridViewRow row in dgvInventory.Rows)
                {
                    string daysLeft = row.Cells["Days Left"].Value?.ToString() ?? "";
                    if (daysLeft == "EXPIRED") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                    else if (daysLeft.Contains("days") && int.TryParse(daysLeft.Replace(" days", ""), out int days) && days < 10)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 200);

                    string bg = row.Cells["Blood Group"].Value?.ToString() ?? "";
                    row.Cells["Blood Group"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    row.Cells["Blood Group"].Style.ForeColor = Color.FromArgb(180, 0, 0);
                }

                lblCount.Text = $"Total: {inventories.Count} record(s)";
                if (lblStatusBar != null) lblStatusBar.Text = $"  ✅ {inventories.Count} record(s) loaded  |  {DateTime.Now:hh:mm tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new BloodInventoryForm(null);
            if (form.ShowDialog() == DialogResult.OK) LoadInventory();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0) { MessageBox.Show("Please select a record to edit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int id = Convert.ToInt32(dgvInventory.SelectedRows[0].Cells["InventoryID"].Value);
            var inv = inventoryService.GetInventoryById(id);
            if (inv == null) return;
            var form = new BloodInventoryForm(inv);
            if (form.ShowDialog() == DialogResult.OK) LoadInventory();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0) { MessageBox.Show("Please select a record.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show("Delete this inventory record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvInventory.SelectedRows[0].Cells["InventoryID"].Value);
                if (inventoryService.DeleteInventory(id)) { MessageBox.Show("Deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadInventory(); }
                else MessageBox.Show("Failed to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ---- Inventory Add/Edit Form ----
    public class BloodInventoryForm : Form
    {
        private BloodInventory inventory;
        private bool isEdit;
        private BloodInventoryService inventoryService = new BloodInventoryService();
        private DonorService donorService = new DonorService();

        private ComboBox cboBloodGroup, cboDonor, cboStatus;
        private NumericUpDown nudUnits;
        private DateTimePicker dtpCollection, dtpExpiry;
        private Button btnSave, btnCancel;

        public BloodInventoryForm(BloodInventory existing)
        {
            inventory = existing;
            isEdit = inventory != null;
            InitializeComponents();
            LoadDonors();
            if (isEdit) PopulateFields();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Blood Stock" : "Add Blood Stock";
            this.Size = new Size(460, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(460, 50), BackColor = Color.FromArgb(180, 0, 0) };
            var lblT = new Label { Text = isEdit ? "✏️  Edit Blood Stock" : "➕  Add Blood Stock", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
            titleBar.Controls.Add(lblT);

            int y = 65;
            var lblBG = new Label { Text = "Blood Group *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboBloodGroup = new ComboBox { Location = new Point(30, y + 22), Size = new Size(180, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBloodGroup.Items.AddRange(new object[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });

            var lblUnits = new Label { Text = "Units *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(240, y) };
            nudUnits = new NumericUpDown { Location = new Point(240, y + 22), Size = new Size(150, 30), Font = new Font("Segoe UI", 11), Minimum = 1, Maximum = 100, Value = 1 };

            y += 65;
            var lblColl = new Label { Text = "Collection Date *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            dtpCollection = new DateTimePicker { Location = new Point(30, y + 22), Size = new Size(180, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            dtpCollection.ValueChanged += (s, e) => { dtpExpiry.Value = dtpCollection.Value.AddDays(42); };

            var lblExp = new Label { Text = "Expiry Date *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(240, y) };
            dtpExpiry = new DateTimePicker { Location = new Point(240, y + 22), Size = new Size(150, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            dtpExpiry.Value = DateTime.Now.AddDays(42);

            y += 65;
            var lblDonor = new Label { Text = "Donor (optional)", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboDonor = new ComboBox { Location = new Point(30, y + 22), Size = new Size(360, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };

            y += 65;
            var lblStatus = new Label { Text = "Status *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboStatus = new ComboBox { Location = new Point(30, y + 22), Size = new Size(180, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Available", "Reserved", "Used", "Expired" });
            cboStatus.SelectedIndex = 0;

            y += 65;
            btnSave = new Button { Text = isEdit ? "✅  Update" : "✅  Save", Location = new Point(30, y), Size = new Size(150, 42), BackColor = Color.FromArgb(180, 0, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "❌  Cancel", Location = new Point(200, y), Size = new Size(120, 42), BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { titleBar, lblBG, cboBloodGroup, lblUnits, nudUnits, lblColl, dtpCollection, lblExp, dtpExpiry, lblDonor, cboDonor, lblStatus, cboStatus, btnSave, btnCancel });
            this.Height = y + 100;
        }

        private void LoadDonors()
        {
            cboDonor.Items.Clear();
            cboDonor.Items.Add(new { Text = "-- No Donor --", Value = (int?)null });
            var donors = donorService.GetAllDonors();
            foreach (var d in donors)
                cboDonor.Items.Add(new DonorItem { Text = $"{d.FullName} ({d.BloodGroup})", Value = d.DonorID });
            cboDonor.DisplayMember = "Text";
            cboDonor.SelectedIndex = 0;
        }

        private void PopulateFields()
        {
            cboBloodGroup.SelectedItem = inventory.BloodGroup;
            nudUnits.Value = inventory.Units;
            dtpCollection.Value = inventory.CollectionDate;
            dtpExpiry.Value = inventory.ExpiryDate;
            cboStatus.SelectedItem = inventory.Status;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboBloodGroup.SelectedIndex < 0) { MessageBox.Show("Please select blood group.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                int? donorId = null;
                if (cboDonor.SelectedItem is DonorItem di) donorId = di.Value;

                var inv = new BloodInventory
                {
                    BloodGroup = cboBloodGroup.SelectedItem.ToString(),
                    Units = (int)nudUnits.Value,
                    CollectionDate = dtpCollection.Value,
                    ExpiryDate = dtpExpiry.Value,
                    DonorID = donorId,
                    Status = cboStatus.SelectedItem?.ToString() ?? "Available"
                };

                bool success;
                if (isEdit) { inv.InventoryID = inventory.InventoryID; success = inventoryService.UpdateInventory(inv); }
                else success = inventoryService.AddInventory(inv);

                if (success) { MessageBox.Show(isEdit ? "Updated!" : "Added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                else MessageBox.Show("Operation failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    public class DonorItem
    {
        public string Text { get; set; }
        public int? Value { get; set; }
        public override string ToString() => Text;
    }
}
