using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class RecipientManagementControl : UserControl
    {
        private DataGridView dgvRecipients;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnSearch;
        private Label lblTitle, lblCount, lblStatusBar;
        private bool _sortAsc = true; private string _lastSort = "";
        private RecipientService recipientService = new RecipientService();

        public RecipientManagementControl()
        {
            InitializeComponents();
            LoadRecipients();
        }

        private void InitializeComponents()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            lblTitle = new Label { Text = "👥  Recipient Management", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), AutoSize = true, Location = new Point(0, 5) };

            var toolbar = new Panel { Location = new Point(0, 45), Size = new Size(960, 55), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            txtSearch = new TextBox { Size = new Size(280, 30), Location = new Point(10, 12), Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            btnSearch = CreateButton("🔍 Search", Color.FromArgb(52, 152, 219), new Point(300, 12), new Size(100, 30));
            btnSearch.Click += (s, e) => SearchRecipients();
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SearchRecipients(); };

            btnAdd = CreateButton("➕ Add Recipient", Color.FromArgb(46, 204, 113), new Point(620, 12), new Size(140, 30));
            btnEdit = CreateButton("✏️ Edit", Color.FromArgb(52, 152, 219), new Point(770, 12), new Size(90, 30));
            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(231, 76, 60), new Point(870, 12), new Size(90, 30));
            btnRefresh = CreateButton("🔄", Color.FromArgb(149, 165, 166), new Point(970, 12), new Size(40, 30));

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadRecipients(); };

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnAdd, btnEdit, btnDelete, btnRefresh });

            dgvRecipients = new DataGridView
            {
                Location = new Point(0, 110), Size = new Size(985, 430),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 38,
                RowTemplate = { Height = 32 }, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvRecipients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 0, 0);
            dgvRecipients.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecipients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvRecipients.EnableHeadersVisualStyles = false;
            dgvRecipients.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 240, 240);
            dgvRecipients.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 0, 0);
            dgvRecipients.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvRecipients.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);

            lblCount = new Label { Text = "Total: 0 recipients", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 548) };
            lblStatusBar = new Label { Text = "  Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.White, BackColor = Color.FromArgb(40,40,40), Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,0,0) };
            dgvRecipients.ColumnHeaderMouseClick += (s,e) => {
                string col = dgvRecipients.Columns[e.ColumnIndex].HeaderText;
                if (_lastSort == col) _sortAsc = !_sortAsc; else { _lastSort = col; _sortAsc = true; }
                if (dgvRecipients.DataSource is System.Data.DataTable dt2) { dt2.DefaultView.Sort = $"[{col}] {(_sortAsc?"ASC":"DESC")}"; dgvRecipients.DataSource = dt2.DefaultView.ToTable(); dgvRecipients.Columns[0].Visible = false; }
                lblStatusBar.Text = $"  🔃 Sorted by {col}";
            };
            this.Controls.AddRange(new Control[] { lblTitle, toolbar, dgvRecipients, lblCount, lblStatusBar });
        }

        private Button CreateButton(string text, Color backColor, Point location, Size size)
        {
            var btn = new Button { Text = text, Location = location, Size = size, BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadRecipients()
        {
            try
            {
                var list = recipientService.GetAllRecipients();
                var dt = new System.Data.DataTable();
                dt.Columns.Add("RecipientID", typeof(int));
                dt.Columns.Add("Full Name"); dt.Columns.Add("CNIC"); dt.Columns.Add("Blood Group");
                dt.Columns.Add("Gender"); dt.Columns.Add("Phone"); dt.Columns.Add("Hospital"); dt.Columns.Add("Condition");

                foreach (var r in list)
                    dt.Rows.Add(r.RecipientID, r.FullName, r.CNIC, r.BloodGroup, r.Gender, r.PhoneNumber, r.HospitalName, r.MedicalCondition);

                dgvRecipients.DataSource = dt;
                dgvRecipients.Columns["RecipientID"].Visible = false;
                dgvRecipients.Columns["Blood Group"].DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                dgvRecipients.Columns["Blood Group"].DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                lblCount.Text = $"Total: {list.Count} recipient(s)";
                lblStatusBar.Text = $"  ✅ {list.Count} recipient(s) loaded  |  {DateTime.Now:hh:mm tt}";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void SearchRecipients()
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text)) { LoadRecipients(); return; }
            try
            {
                var list = recipientService.SearchRecipients(txtSearch.Text.Trim());
                var dt = new System.Data.DataTable();
                dt.Columns.Add("RecipientID", typeof(int)); dt.Columns.Add("Full Name"); dt.Columns.Add("CNIC");
                dt.Columns.Add("Blood Group"); dt.Columns.Add("Gender"); dt.Columns.Add("Phone"); dt.Columns.Add("Hospital"); dt.Columns.Add("Condition");
                foreach (var r in list)
                    dt.Rows.Add(r.RecipientID, r.FullName, r.CNIC, r.BloodGroup, r.Gender, r.PhoneNumber, r.HospitalName, r.MedicalCondition);
                dgvRecipients.DataSource = dt;
                dgvRecipients.Columns["RecipientID"].Visible = false;
                lblCount.Text = $"Found: {list.Count} recipient(s)";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new RecipientForm(null);
            if (form.ShowDialog() == DialogResult.OK) LoadRecipients();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRecipients.SelectedRows.Count == 0) { MessageBox.Show("Select a recipient.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int id = Convert.ToInt32(dgvRecipients.SelectedRows[0].Cells["RecipientID"].Value);
            var r = recipientService.GetRecipientById(id);
            if (r == null) return;
            var form = new RecipientForm(r);
            if (form.ShowDialog() == DialogResult.OK) LoadRecipients();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRecipients.SelectedRows.Count == 0) { MessageBox.Show("Select a recipient.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string name = dgvRecipients.SelectedRows[0].Cells["Full Name"].Value.ToString();
            if (MessageBox.Show($"Delete '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int id = Convert.ToInt32(dgvRecipients.SelectedRows[0].Cells["RecipientID"].Value);
                if (recipientService.DeleteRecipient(id)) { MessageBox.Show("Deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadRecipients(); }
                else MessageBox.Show("Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ---- Recipient Form ----
    public class RecipientForm : Form
    {
        private Recipient recipient;
        private bool isEdit;
        private RecipientService service = new RecipientService();
        private TextBox txtName, txtCNIC, txtPhone, txtEmail, txtAddress, txtHospital, txtCondition;
        private ComboBox cboBloodGroup, cboGender;
        private DateTimePicker dtpDOB;
        private Button btnSave, btnCancel;

        public RecipientForm(Recipient existing)
        {
            recipient = existing;
            isEdit = recipient != null;
            InitializeComponents();
            if (isEdit) Populate();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Recipient" : "Add Recipient";
            this.Size = new Size(520, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 50), BackColor = Color.FromArgb(180, 0, 0) };
            var lblT = new Label { Text = isEdit ? "✏️  Edit Recipient" : "➕  Add Recipient", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
            titleBar.Controls.Add(lblT);

            int y = 70;
            AddField("Full Name *", out txtName, y); y += 65;
            AddField("CNIC *", out txtCNIC, y); y += 65;

            var lBG = new Label { Text = "Blood Group *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboBloodGroup = new ComboBox { Location = new Point(30, y + 22), Size = new Size(180, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBloodGroup.Items.AddRange(new object[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });
            var lGen = new Label { Text = "Gender *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(240, y) };
            cboGender = new ComboBox { Location = new Point(240, y + 22), Size = new Size(180, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            this.Controls.AddRange(new Control[] { lBG, cboBloodGroup, lGen, cboGender });
            y += 65;

            var lDOB = new Label { Text = "Date of Birth *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            dtpDOB = new DateTimePicker { Location = new Point(30, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            dtpDOB.Value = DateTime.Now.AddYears(-30);
            this.Controls.AddRange(new Control[] { lDOB, dtpDOB });
            y += 65;

            AddField("Phone *", out txtPhone, y); y += 65;
            AddField("Email", out txtEmail, y); y += 65;
            AddField("Hospital Name", out txtHospital, y); y += 65;
            AddField("Medical Condition", out txtCondition, y); y += 65;

            btnSave = new Button { Text = isEdit ? "✅  Update" : "✅  Save", Location = new Point(30, y), Size = new Size(150, 42), BackColor = Color.FromArgb(180, 0, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "❌  Cancel", Location = new Point(200, y), Size = new Size(120, 42), BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { titleBar, btnSave, btnCancel });
            this.Height = y + 100;
        }

        private void AddField(string label, out TextBox txt, int y)
        {
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            txt = new TextBox { Location = new Point(30, y + 22), Size = new Size(430, 32), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(lbl); this.Controls.Add(txt);
        }

        private void Populate()
        {
            txtName.Text = recipient.FullName; txtCNIC.Text = recipient.CNIC;
            cboBloodGroup.SelectedItem = recipient.BloodGroup; cboGender.SelectedItem = recipient.Gender;
            dtpDOB.Value = recipient.DateOfBirth; txtPhone.Text = recipient.PhoneNumber;
            txtEmail.Text = recipient.Email; txtHospital.Text = recipient.HospitalName; txtCondition.Text = recipient.MedicalCondition;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCNIC.Text) || string.IsNullOrWhiteSpace(txtPhone.Text) || cboBloodGroup.SelectedIndex < 0)
            { MessageBox.Show("Fill required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!isEdit && service.RecipientExists(txtCNIC.Text.Trim()))
            { MessageBox.Show("Recipient with this CNIC already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var r = new Recipient
                {
                    FullName = txtName.Text.Trim(), CNIC = txtCNIC.Text.Trim(),
                    BloodGroup = cboBloodGroup.SelectedItem.ToString(), DateOfBirth = dtpDOB.Value,
                    Gender = cboGender.SelectedItem?.ToString() ?? "Male", PhoneNumber = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(), HospitalName = txtHospital.Text.Trim(), MedicalCondition = txtCondition.Text.Trim()
                };
                bool success;
                if (isEdit) { r.RecipientID = recipient.RecipientID; success = service.UpdateRecipient(r); }
                else success = service.AddRecipient(r);

                if (success) { MessageBox.Show(isEdit ? "Updated!" : "Added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                else MessageBox.Show("Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
