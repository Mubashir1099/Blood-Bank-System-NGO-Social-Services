using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class VolunteerForm : Form
    {
        private Volunteer volunteer;
        private bool isEdit;
        private VolunteerService volunteerService = new VolunteerService();

        private TextBox txtFullName, txtPhone, txtEmail, txtAddress, txtSkills;
        private Button btnSave, btnCancel;

        public VolunteerForm(Volunteer existingVolunteer)
        {
            volunteer = existingVolunteer;
            isEdit = volunteer != null;
            InitializeComponents();
            if (isEdit) PopulateFields();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Volunteer" : "Add New Volunteer";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 50), BackColor = Color.FromArgb(22, 27, 38) };
            var lblTitle = new Label
            {
                Text = isEdit ? "✏️  Edit Volunteer Information" : "➕  Add New Volunteer",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            titleBar.Controls.Add(lblTitle);

            int y = 70;
            AddField("Full Name *", out txtFullName, y); y += 65;
            AddField("Phone Number *", out txtPhone, y); y += 65;
            AddField("Email", out txtEmail, y); y += 65;
            AddField("Address", out txtAddress, y); y += 65;
            AddField("Skills (e.g. Coordination, Medical, Logistics)", out txtSkills, y); y += 65;

            btnSave = new Button
            {
                Text = isEdit ? "✅  Update Volunteer" : "✅  Save Volunteer",
                Location = new Point(30, y),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(25, 135, 84), // NGO green-ish/accent
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
                titleBar, btnSave, btnCancel
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
            txtFullName.Text = volunteer.FullName;
            txtPhone.Text = volunteer.PhoneNumber;
            txtEmail.Text = volunteer.Email;
            txtAddress.Text = volunteer.Address;
            txtSkills.Text = volunteer.Skills;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var v = new Volunteer
                {
                    FullName = txtFullName.Text.Trim(),
                    PhoneNumber = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    Skills = txtSkills.Text.Trim()
                };

                bool success;
                if (isEdit)
                {
                    v.VolunteerID = volunteer.VolunteerID;
                    success = volunteerService.UpdateVolunteer(v);
                }
                else
                    success = volunteerService.AddVolunteer(v);

                if (success)
                {
                    MessageBox.Show(isEdit ? "Volunteer updated successfully!" : "Volunteer added successfully!",
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
