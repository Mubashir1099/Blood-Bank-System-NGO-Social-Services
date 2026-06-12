using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class BloodRequestControl : UserControl
    {
        private DataGridView dgvRequests;
        private Button btnAdd, btnEdit, btnApprove, btnReject, btnDelete, btnRefresh;
        private ComboBox cboStatusFilter;
        private Label lblTitle, lblCount, lblStatusBar;
        private bool _sortAsc = true; private string _lastSort = "";
        private BloodRequestService requestService = new BloodRequestService();
        private BloodInventoryService inventoryService = new BloodInventoryService();
        private User currentUser;

        public BloodRequestControl(User user)
        {
            currentUser = user;
            InitializeComponents();
            LoadRequests();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            lblTitle = new Label { Text = "📋  Blood Request Management", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), AutoSize = true, Location = new Point(0, 5) };

            var toolbar = new Panel { Location = new Point(0, 45), Size = new Size(985, 55), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            var lblF = new Label { Text = "Status:", Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(10, 18), ForeColor = Color.Gray };
            cboStatusFilter = new ComboBox { Location = new Point(60, 13), Size = new Size(120, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatusFilter.Items.AddRange(new object[] { "All", "Pending", "Approved", "Rejected" });
            cboStatusFilter.SelectedIndex = 0;
            cboStatusFilter.SelectedIndexChanged += (s, e) => LoadRequests();

            btnAdd = CreateButton("➕ New Request", Color.FromArgb(46, 204, 113), new Point(200, 12), new Size(130, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(340, 12), new Size(80, 30));
            btnApprove = CreateButton("✅ Approve", Color.FromArgb(39, 174, 96), new Point(430, 12), new Size(100, 30));
            btnReject = CreateButton("❌ Reject", Color.FromArgb(192, 57, 43), new Point(540, 12), new Size(90, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(149, 165, 166), new Point(640, 12), new Size(90, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(740, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnApprove.Click += BtnApprove_Click;
            btnReject.Click += BtnReject_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { cboStatusFilter.SelectedIndex = 0; LoadRequests(); };

            toolbar.Controls.AddRange(new Control[] { lblF, cboStatusFilter, btnAdd, btnEdit, btnApprove, btnReject, btnDelete, btnRefresh });

            dgvRequests = new DataGridView
            {
                Location = new Point(0, 110), Size = new Size(985, 430),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 }, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvRequests.EnableHeadersVisualStyles = false;
            dgvRequests.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            dgvRequests.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            dgvRequests.DefaultCellStyle.SelectionForeColor = Color.White;

            lblCount = new Label { Text = "Total: 0 requests", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 548) };
            lblStatusBar = new Label { Text = "  Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.White, BackColor = Color.FromArgb(40,40,40), Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,0,0) };
            dgvRequests.ColumnHeaderMouseClick += (s,e) => {
                string col = dgvRequests.Columns[e.ColumnIndex].HeaderText;
                if (_lastSort == col) _sortAsc = !_sortAsc; else { _lastSort = col; _sortAsc = true; }
                if (dgvRequests.DataSource is System.Data.DataTable dt2) { dt2.DefaultView.Sort = $"[{col}] {(_sortAsc?"ASC":"DESC")}"; dgvRequests.DataSource = dt2.DefaultView.ToTable(); }
                lblStatusBar.Text = $"  🔃 Sorted by {col}";
            };
            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvRequests, lblCount, lblStatusBar });
        }

        private Button CreateButton(string text, Color backColor, Point location, Size size)
        {
            var btn = new Button { Text = text, Location = location, Size = size, BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadRequests()
        {
            try
            {
                var list = cboStatusFilter.SelectedIndex == 0
                    ? requestService.GetAllRequests()
                    : requestService.GetRequestsByStatus(cboStatusFilter.SelectedItem.ToString());

                var dt = new System.Data.DataTable();
                dt.Columns.Add("RequestID", typeof(int));
                dt.Columns.Add("Recipient"); dt.Columns.Add("Blood Group"); dt.Columns.Add("Units");
                dt.Columns.Add("Urgency"); dt.Columns.Add("Request Date"); dt.Columns.Add("Required By");
                dt.Columns.Add("Status"); dt.Columns.Add("Approved By");

                foreach (var r in list)
                    dt.Rows.Add(r.RequestID, r.RecipientName, r.BloodGroup, r.UnitsRequired,
                        r.UrgencyLevel, r.RequestDate.ToString("dd-MMM-yyyy"),
                        r.RequiredByDate.HasValue ? r.RequiredByDate.Value.ToString("dd-MMM-yyyy") : "N/A",
                        r.Status, r.ApprovedBy);

                dgvRequests.DataSource = dt;
                dgvRequests.Columns["RequestID"].Visible = false;

                // Color code by status
                foreach (DataGridViewRow row in dgvRequests.Rows)
                {
                    string status = row.Cells["Status"].Value?.ToString() ?? "";
                    if (status == "Approved") row.Cells["Status"].Style.ForeColor = Color.FromArgb(39, 174, 96);
                    else if (status == "Rejected") row.Cells["Status"].Style.ForeColor = Color.Red;
                    else row.Cells["Status"].Style.ForeColor = Color.FromArgb(243, 156, 18);

                    row.Cells["Status"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                    string urgency = row.Cells["Urgency"].Value?.ToString() ?? "";
                    if (urgency == "Critical") row.Cells["Urgency"].Style.ForeColor = Color.Red;
                    else if (urgency == "High") row.Cells["Urgency"].Style.ForeColor = Color.OrangeRed;
                    row.Cells["Blood Group"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    row.Cells["Blood Group"].Style.ForeColor = Color.FromArgb(180, 0, 0);
                }

                lblCount.Text = $"Total: {list.Count} request(s)";
                if (lblStatusBar != null) lblStatusBar.Text = $"  ✅ {list.Count} request(s) loaded  |  {DateTime.Now:hh:mm tt}";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new BloodRequestForm(null);
            if (form.ShowDialog() == DialogResult.OK) LoadRequests();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) { MessageBox.Show("Select a request.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string status = dgvRequests.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
            if (status != "Pending") { MessageBox.Show("Only Pending requests can be edited.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int id = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
            var req = requestService.GetRequestById(id);
            if (req == null) return;
            var form = new BloodRequestForm(req);
            if (form.ShowDialog() == DialogResult.OK) LoadRequests();
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) { MessageBox.Show("Select a request to approve.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string status = dgvRequests.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
            if (status != "Pending") { MessageBox.Show("Only Pending requests can be approved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            int id = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
            var req = requestService.GetRequestById(id);

            int available = inventoryService.GetAvailableUnits(req.BloodGroup);
            if (available < req.UnitsRequired)
            {
                MessageBox.Show($"Insufficient stock!\nRequired: {req.UnitsRequired} units of {req.BloodGroup}\nAvailable: {available} units",
                    "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Approve this request and deduct {req.UnitsRequired} units of {req.BloodGroup}?",
                "Confirm Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (requestService.ApproveRequest(id, currentUser.FullName))
                {
                    inventoryService.DeductUnits(req.BloodGroup, req.UnitsRequired);
                    MessageBox.Show("Request approved and blood inventory updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRequests();
                }
                else MessageBox.Show("Failed to approve.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) { MessageBox.Show("Select a request.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string status = dgvRequests.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
            if (status != "Pending") { MessageBox.Show("Only Pending requests can be rejected.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            if (MessageBox.Show("Reject this blood request?", "Confirm Reject", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                if (requestService.RejectRequest(id)) { MessageBox.Show("Request rejected.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadRequests(); }
                else MessageBox.Show("Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) { MessageBox.Show("Select a request.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show("Delete this request?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                if (requestService.DeleteRequest(id)) { MessageBox.Show("Deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadRequests(); }
                else MessageBox.Show("Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ---- Blood Request Form ----
    public class BloodRequestForm : Form
    {
        private BloodRequest request;
        private bool isEdit;
        private BloodRequestService requestService = new BloodRequestService();
        private RecipientService recipientService = new RecipientService();

        private ComboBox cboRecipient, cboBloodGroup, cboUrgency;
        private NumericUpDown nudUnits;
        private DateTimePicker dtpRequired;
        private CheckBox chkHasRequiredDate;
        private TextBox txtNotes;
        private Button btnSave, btnCancel;

        public BloodRequestForm(BloodRequest existing)
        {
            request = existing;
            isEdit = request != null;
            InitializeComponents();
            LoadRecipients();
            if (isEdit) Populate();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Blood Request" : "New Blood Request";
            this.Size = new Size(480, 530);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(480, 50), BackColor = Color.FromArgb(180, 0, 0) };
            var lblT = new Label { Text = isEdit ? "✏️  Edit Blood Request" : "📋  New Blood Request", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
            titleBar.Controls.Add(lblT);

            int y = 65;
            var lR = new Label { Text = "Recipient *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboRecipient = new ComboBox { Location = new Point(30, y + 22), Size = new Size(400, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            y += 65;

            var lBG = new Label { Text = "Blood Group *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboBloodGroup = new ComboBox { Location = new Point(30, y + 22), Size = new Size(150, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBloodGroup.Items.AddRange(new object[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });

            var lUnits = new Label { Text = "Units Required *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(210, y) };
            nudUnits = new NumericUpDown { Location = new Point(210, y + 22), Size = new Size(100, 30), Font = new Font("Segoe UI", 11), Minimum = 1, Maximum = 50, Value = 1 };

            var lUrg = new Label { Text = "Urgency *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(330, y) };
            cboUrgency = new ComboBox { Location = new Point(330, y + 22), Size = new Size(100, 30), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cboUrgency.Items.AddRange(new object[] { "Normal", "High", "Critical" });
            cboUrgency.SelectedIndex = 0;
            y += 65;

            chkHasRequiredDate = new CheckBox { Text = "Set Required By Date", Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 9) };
            dtpRequired = new DateTimePicker { Location = new Point(220, y - 3), Size = new Size(200, 30), Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Short, Enabled = false };
            chkHasRequiredDate.CheckedChanged += (s, e) => dtpRequired.Enabled = chkHasRequiredDate.Checked;
            y += 40;

            var lNotes = new Label { Text = "Notes", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            txtNotes = new TextBox { Location = new Point(30, y + 22), Size = new Size(400, 70), Font = new Font("Segoe UI", 10), Multiline = true, BorderStyle = BorderStyle.FixedSingle };
            y += 100;

            btnSave = new Button { Text = isEdit ? "✅  Update" : "✅  Submit Request", Location = new Point(30, y), Size = new Size(180, 42), BackColor = Color.FromArgb(180, 0, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "❌  Cancel", Location = new Point(230, y), Size = new Size(120, 42), BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { titleBar, lR, cboRecipient, lBG, cboBloodGroup, lUnits, nudUnits, lUrg, cboUrgency, chkHasRequiredDate, dtpRequired, lNotes, txtNotes, btnSave, btnCancel });
            this.Height = y + 100;
        }

        private void LoadRecipients()
        {
            var list = recipientService.GetAllRecipients();
            foreach (var r in list)
                cboRecipient.Items.Add(new RecipientItem { Text = $"{r.FullName} ({r.BloodGroup})", Value = r.RecipientID, BloodGroup = r.BloodGroup });
            cboRecipient.DisplayMember = "Text";
            cboRecipient.SelectedIndexChanged += (s, e) =>
            {
                if (cboRecipient.SelectedItem is RecipientItem ri) cboBloodGroup.SelectedItem = ri.BloodGroup;
            };
        }

        private void Populate()
        {
            foreach (RecipientItem item in cboRecipient.Items)
                if (item.Value == request.RecipientID) { cboRecipient.SelectedItem = item; break; }
            cboBloodGroup.SelectedItem = request.BloodGroup;
            nudUnits.Value = request.UnitsRequired;
            cboUrgency.SelectedItem = request.UrgencyLevel;
            if (request.RequiredByDate.HasValue) { chkHasRequiredDate.Checked = true; dtpRequired.Value = request.RequiredByDate.Value; }
            txtNotes.Text = request.Notes;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboRecipient.SelectedIndex < 0 || cboBloodGroup.SelectedIndex < 0)
            { MessageBox.Show("Fill required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var ri = (RecipientItem)cboRecipient.SelectedItem;
                var req = new BloodRequest
                {
                    RecipientID = ri.Value,
                    BloodGroup = cboBloodGroup.SelectedItem.ToString(),
                    UnitsRequired = (int)nudUnits.Value,
                    UrgencyLevel = cboUrgency.SelectedItem?.ToString() ?? "Normal",
                    RequestDate = DateTime.Now,
                    RequiredByDate = chkHasRequiredDate.Checked ? dtpRequired.Value : (DateTime?)null,
                    Notes = txtNotes.Text.Trim()
                };

                bool success;
                if (isEdit) { req.RequestID = request.RequestID; success = requestService.UpdateRequest(req); }
                else success = requestService.AddRequest(req);

                if (success) { MessageBox.Show(isEdit ? "Updated!" : "Request submitted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                else MessageBox.Show("Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    public class RecipientItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public string BloodGroup { get; set; }
        public override string ToString() => Text;
    }
}
