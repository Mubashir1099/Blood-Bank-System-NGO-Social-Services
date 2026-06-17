using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class AssignVolunteersForm : Form
    {
        private int campId;
        private string campName;
        private CampService campService = new CampService();
        private VolunteerService volunteerService = new VolunteerService();

        private CheckedListBox clbVolunteers;
        private Button btnSave, btnCancel;
        private Label lblCampTitle;

        // Custom list item class to bind volunteer object
        private class VolunteerItem
        {
            public Volunteer Volunteer { get; set; }
            public VolunteerItem(Volunteer v) { Volunteer = v; }
            public override string ToString() => $"{Volunteer.FullName} ({Volunteer.Skills})";
        }

        public AssignVolunteersForm(int campId, string campName)
        {
            this.campId = campId;
            this.campName = campName;
            InitializeComponents();
            LoadVolunteers();
        }

        private void InitializeComponents()
        {
            this.Text = "Assign Volunteers";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(450, 60), BackColor = Color.FromArgb(22, 27, 38) };
            lblCampTitle = new Label
            {
                Text = $"Assign Volunteers to Camp:\n{campName}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                Size = new Size(420, 40)
            };
            titleBar.Controls.Add(lblCampTitle);

            var lblInstruction = new Label
            {
                Text = "Check the volunteers to assign to this camp:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(25, 75),
                Size = new Size(400, 20)
            };

            clbVolunteers = new CheckedListBox
            {
                Location = new Point(25, 100),
                Size = new Size(385, 290),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };

            btnSave = new Button
            {
                Text = "💾  Save Assignments",
                Location = new Point(25, 405),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(220, 405),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { titleBar, lblInstruction, clbVolunteers, btnSave, btnCancel });
        }

        private void LoadVolunteers()
        {
            try
            {
                var allVolunteers = volunteerService.GetAllVolunteers();
                var assignedVolunteers = campService.GetVolunteersForCamp(campId);

                // Create lookup set of assigned IDs
                var assignedIds = new HashSet<int>();
                foreach (var av in assignedVolunteers)
                    assignedIds.Add(av.VolunteerID);

                clbVolunteers.Items.Clear();
                foreach (var v in allVolunteers)
                {
                    var item = new VolunteerItem(v);
                    int index = clbVolunteers.Items.Add(item);
                    if (assignedIds.Contains(v.VolunteerID))
                        clbVolunteers.SetItemChecked(index, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteers list: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var currentAssigned = campService.GetVolunteersForCamp(campId);
                var currentlyAssignedIds = new HashSet<int>();
                foreach (var av in currentAssigned)
                    currentlyAssignedIds.Add(av.VolunteerID);

                var checkedIds = new HashSet<int>();

                // 1. Process Checked Items (Add if not already assigned)
                for (int i = 0; i < clbVolunteers.Items.Count; i++)
                {
                    var item = (VolunteerItem)clbVolunteers.Items[i];
                    int volunteerId = item.Volunteer.VolunteerID;

                    if (clbVolunteers.GetItemChecked(i))
                    {
                        checkedIds.Add(volunteerId);
                        if (!currentlyAssignedIds.Contains(volunteerId))
                        {
                            campService.AssignVolunteerToCamp(campId, volunteerId);
                        }
                    }
                }

                // 2. Process Unchecked Items (Remove if previously assigned)
                foreach (var volunteerId in currentlyAssignedIds)
                {
                    if (!checkedIds.Contains(volunteerId))
                    {
                        campService.RemoveVolunteerFromCamp(campId, volunteerId);
                    }
                }

                MessageBox.Show("Volunteers assigned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving assignments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
