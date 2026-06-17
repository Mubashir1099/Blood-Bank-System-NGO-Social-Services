using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class CampForm : Form
    {
        private Camp camp;
        private bool isEdit;
        private CampService campService = new CampService();

        private TextBox txtCampName, txtLocation, txtOrganizer;
        private NumericUpDown nudTargetUnits;
        private DateTimePicker dtpCampDate;
        private Button btnSave, btnCancel;

        public CampForm(Camp existingCamp)
        {
            camp = existingCamp;
            isEdit = camp != null;
            InitializeComponents();
            if (isEdit) PopulateFields();
        }

        private void InitializeComponents()
        {
            this.Text = isEdit ? "Edit Blood Camp" : "Add New Blood Camp";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 50), BackColor = Color.FromArgb(180, 0, 0) };
            var lblTitle = new Label
            {
                Text = isEdit ? "✏️  Edit Blood Camp Details" : "➕  Add New Blood Camp",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            titleBar.Controls.Add(lblTitle);

            int y = 70;
            AddField("Camp Name *", out txtCampName, y); y += 65;
            AddField("Location / Venue *", out txtLocation, y); y += 65;

            var lblDate = new Label { Text = "Camp Date *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) };
            dtpCampDate = new DateTimePicker { Location = new Point(30, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            dtpCampDate.Value = DateTime.Now.AddDays(7); // default 1 week from now
            this.Controls.AddRange(new Control[] { lblDate, dtpCampDate });

            var lblTarget = new Label { Text = "Target Units *", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(260, y) };
            nudTargetUnits = new NumericUpDown { Location = new Point(260, y + 22), Size = new Size(200, 30), Font = new Font("Segoe UI", 11), Minimum = 1, Maximum = 1000, Value = 50 };
            this.Controls.AddRange(new Control[] { lblTarget, nudTargetUnits });

            y += 65;
            AddField("Organizer Name / Sponsor *", out txtOrganizer, y); y += 65;

            btnSave = new Button
            {
                Text = isEdit ? "✅  Update Camp" : "✅  Save Camp",
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
            txtCampName.Text = camp.CampName;
            txtLocation.Text = camp.Location;
            dtpCampDate.Value = camp.CampDate;
            nudTargetUnits.Value = camp.TargetUnits;
            txtOrganizer.Text = camp.OrganizerName;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCampName.Text) || string.IsNullOrWhiteSpace(txtLocation.Text) || string.IsNullOrWhiteSpace(txtOrganizer.Text))
            {
                MessageBox.Show("Please fill all required fields (marked with *).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var c = new Camp
                {
                    CampName = txtCampName.Text.Trim(),
                    Location = txtLocation.Text.Trim(),
                    CampDate = dtpCampDate.Value,
                    TargetUnits = (int)nudTargetUnits.Value,
                    OrganizerName = txtOrganizer.Text.Trim()
                };

                bool success;
                if (isEdit)
                {
                    c.CampID = camp.CampID;
                    success = campService.UpdateCamp(c);
                }
                else
                    success = campService.AddCamp(c);

                if (success)
                {
                    MessageBox.Show(isEdit ? "Camp updated successfully!" : "Camp added successfully!",
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
