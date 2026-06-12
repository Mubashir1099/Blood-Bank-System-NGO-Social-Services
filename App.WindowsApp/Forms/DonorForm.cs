using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class DonorForm : Form
    {
        private Donor donor;
        private bool isEdit;
        private DonorService donorService = new DonorService();

        private TextBox txtFullName, txtCNIC, txtPhone, txtEmail, txtAddress;
        private ComboBox cboBloodGroup, cboGender;
        private DateTimePicker dtpDOB, dtpLastDonation;
        private CheckBox chkHasLastDonation;
        private Button btnSave, btnCancel;

        public DonorForm(Donor existingDonor)
        {
            donor = existingDonor;
            isEdit = donor != null;
            InitializeComponents();
            if (isEdit) PopulateFields();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Donor" : "Add New Donor";
            this.Size = new Size(520, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 50), BackColor = Color.FromArgb(180, 0, 0) };
            var lblTitle = new Label
            {
                Text = isEdit ? "✏️  Edit Donor Information" : "➕  Add New Donor",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            titleBar.Controls.Add(lblTitle);

            int y = 70;
            AddField("Full Name *", out txtFullName, y); y += 65;
            AddField("CNIC * (e.g. 42101-1234567-1)", out txtCNIC, y); y += 65;

            var lblBG = new Label { Text = "Blood Group *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            cboBloodGroup = new ComboBox { Location = new Point(30, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboBloodGroup.Items.AddRange(new object[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });

            var lblGender = new Label { Text = "Gender *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(260, y) };
            cboGender = new ComboBox { Location = new Point(260, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new object[] { "Male", "Female", "Other" });

            y += 65;
            var lblDOB = new Label { Text = "Date of Birth *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            dtpDOB = new DateTimePicker { Location = new Point(30, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            dtpDOB.Value = DateTime.Now.AddYears(-25);

            y += 65;
            AddField("Phone Number *", out txtPhone, y); y += 65;
            AddField("Email", out txtEmail, y); y += 65;
            AddField("Address", out txtAddress, y); y += 65;

            var lblLD = new Label { Text = "Last Donation Date", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            chkHasLastDonation = new CheckBox { Text = "Has donated before", Location = new Point(250, y), AutoSize = true, Font = new Font("Segoe UI", 9) };
            dtpLastDonation = new DateTimePicker { Location = new Point(30, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short, Enabled = false };
            chkHasLastDonation.CheckedChanged += (s, e) => dtpLastDonation.Enabled = chkHasLastDonation.Checked;

            y += 65;
            btnSave = new Button
            {
                Text = isEdit ? "✅  Update Donor" : "✅  Save Donor",
                Location = new Point(30, y),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(230, y),
                Size = new Size(120, 42),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                titleBar, lblBG, cboBloodGroup, lblGender, cboGender,
                lblDOB, dtpDOB, lblLD, chkHasLastDonation, dtpLastDonation, btnSave, btnCancel
            });
            this.Height = y + 100;
        }

        private void AddField(string label, out TextBox txt, int y)
        {
            var lbl = new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            txt = new TextBox { Location = new Point(30, y + 22), Size = new Size(430, 32), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
        }

        private void PopulateFields()
        {
            txtFullName.Text = donor.FullName;
            txtCNIC.Text = donor.CNIC;
            cboBloodGroup.SelectedItem = donor.BloodGroup;
            cboGender.SelectedItem = donor.Gender;
            dtpDOB.Value = donor.DateOfBirth;
            txtPhone.Text = donor.PhoneNumber;
            txtEmail.Text = donor.Email;
            txtAddress.Text = donor.Address;
            if (donor.LastDonationDate.HasValue)
            {
                chkHasLastDonation.Checked = true;
                dtpLastDonation.Value = donor.LastDonationDate.Value;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtCNIC.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) || cboBloodGroup.SelectedIndex < 0 || cboGender.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!isEdit && donorService.DonorExists(txtCNIC.Text.Trim()))
            {
                MessageBox.Show("A donor with this CNIC already exists.", "Duplicate CNIC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var d = new Donor
                {
                    FullName = txtFullName.Text.Trim(),
                    CNIC = txtCNIC.Text.Trim(),
                    BloodGroup = cboBloodGroup.SelectedItem.ToString(),
                    DateOfBirth = dtpDOB.Value,
                    Gender = cboGender.SelectedItem.ToString(),
                    PhoneNumber = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    LastDonationDate = chkHasLastDonation.Checked ? dtpLastDonation.Value : (DateTime?)null
                };

                bool success;
                if (isEdit)
                {
                    d.DonorID = donor.DonorID;
                    success = donorService.UpdateDonor(d);
                }
                else
                    success = donorService.AddDonor(d);

                if (success)
                {
                    MessageBox.Show(isEdit ? "Donor updated successfully!" : "Donor added successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                    MessageBox.Show("Operation failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
