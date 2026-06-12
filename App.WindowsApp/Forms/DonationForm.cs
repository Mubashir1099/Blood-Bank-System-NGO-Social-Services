using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    /// <summary>
    /// Form to record a new blood donation session.
    /// Opened from the Donor Management panel.
    /// </summary>
    public class DonationForm : Form
    {
        private readonly DonorService donorService = new DonorService();
        private readonly BloodInventoryService inventoryService = new BloodInventoryService();

        private ComboBox cboDonor;
        private Label lblBloodGroup;
        private DateTimePicker dtpDonationDate;
        private NumericUpDown nudUnits;
        private TextBox txtBP, txtHemoglobin, txtNotes;
        private Button btnSave, btnCancel;

        public DonationForm()
        {
            InitializeComponents();
            LoadDonors();
        }

        private void InitializeComponents()
        {
            this.Text = "Record Blood Donation";
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Title bar
            var titleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(500, 52),
                BackColor = Color.FromArgb(180, 0, 0)
            };
            titleBar.Controls.Add(new Label
            {
                Text = "🩸  Record New Donation",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(14, 14)
            });

            int y = 68;

            // Donor
            AddLabel("Select Donor *", 30, y);
            cboDonor = new ComboBox
            {
                Location = new Point(30, y + 22),
                Size = new Size(420, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDonor.SelectedIndexChanged += CboDonor_SelectedIndexChanged;
            y += 62;

            // Blood Group (auto-filled)
            AddLabel("Blood Group", 30, y);
            lblBloodGroup = new Label
            {
                Location = new Point(30, y + 22),
                Size = new Size(160, 30),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 0, 0),
                Text = "—"
            };

            // Donation Date
            AddLabel("Donation Date *", 240, y);
            dtpDonationDate = new DateTimePicker
            {
                Location = new Point(240, y + 22),
                Size = new Size(210, 30),
                Font = new Font("Segoe UI", 10),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            y += 62;

            // Units
            AddLabel("Units Donated *", 30, y);
            nudUnits = new NumericUpDown
            {
                Location = new Point(30, y + 22),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 11),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };

            // BP
            AddLabel("Blood Pressure", 160, y);
            txtBP = new TextBox
            {
                Location = new Point(160, y + 22),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 10),
            };

            // Hemoglobin
            AddLabel("Hemoglobin (g/dL)", 310, y);
            txtHemoglobin = new TextBox
            {
                Location = new Point(310, y + 22),
                Size = new Size(140, 30),
                Font = new Font("Segoe UI", 10),
            };
            y += 62;

            // Notes
            AddLabel("Notes / Remarks", 30, y);
            txtNotes = new TextBox
            {
                Location = new Point(30, y + 22),
                Size = new Size(420, 65),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            y += 98;

            // Info label
            var lblInfo = new Label
            {
                Text = "ℹ  Saving will also add donated units to Blood Inventory.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(30, y)
            };
            y += 28;

            // Buttons
            btnSave = MakeButton("✅  Save Donation", Color.FromArgb(180, 0, 0), new Point(30, y), new Size(185, 42));
            btnSave.Click += BtnSave_Click;

            btnCancel = MakeButton("❌  Cancel", Color.FromArgb(120, 120, 120), new Point(230, y), new Size(120, 42));
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] {
                titleBar, cboDonor, lblBloodGroup,
                dtpDonationDate, nudUnits, txtBP, txtHemoglobin,
                txtNotes, lblInfo, btnSave, btnCancel
            });
            this.Height = y + 90;
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(x, y)
            });
        }

        private Button MakeButton(string text, Color color, Point loc, Size size)
        {
            var b = new Button
            {
                Text = text, Location = loc, Size = size,
                BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadDonors()
        {
            cboDonor.Items.Clear();
            foreach (var d in donorService.GetAllDonors())
                cboDonor.Items.Add(new DonorCombo { Text = $"{d.FullName}  ({d.BloodGroup})", DonorID = d.DonorID, BloodGroup = d.BloodGroup });
            cboDonor.DisplayMember = "Text";
        }

        private void CboDonor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDonor.SelectedItem is DonorCombo dc)
                lblBloodGroup.Text = dc.BloodGroup;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboDonor.SelectedIndex < 0)
            { MessageBox.Show("Please select a donor.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var dc = (DonorCombo)cboDonor.SelectedItem;

            try
            {
                // 1. Insert into Donations table
                string donSql = @"INSERT INTO Donations (DonorID, DonationDate, BloodGroup, UnitsDonated, BloodPressure, Hemoglobin, Notes)
                                  VALUES (@DonorID, @Date, @BG, @Units, @BP, @HB, @Notes)";
                var p = new Microsoft.Data.SqlClient.SqlParameter[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@DonorID", dc.DonorID),
                    new Microsoft.Data.SqlClient.SqlParameter("@Date",    dtpDonationDate.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@BG",      dc.BloodGroup),
                    new Microsoft.Data.SqlClient.SqlParameter("@Units",   (int)nudUnits.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@BP",      string.IsNullOrWhiteSpace(txtBP.Text) ? (object)DBNull.Value : txtBP.Text.Trim()),
                    new Microsoft.Data.SqlClient.SqlParameter("@HB",      string.IsNullOrWhiteSpace(txtHemoglobin.Text) ? (object)DBNull.Value : txtHemoglobin.Text.Trim()),
                    new Microsoft.Data.SqlClient.SqlParameter("@Notes",   string.IsNullOrWhiteSpace(txtNotes.Text) ? (object)DBNull.Value : txtNotes.Text.Trim()),
                };
                DatabaseHelper.ExecuteNonQuery(donSql, p);

                // 2. Update donor's LastDonationDate
                string updSql = "UPDATE Donors SET LastDonationDate=@Date WHERE DonorID=@DonorID";
                DatabaseHelper.ExecuteNonQuery(updSql, new Microsoft.Data.SqlClient.SqlParameter[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@Date", dtpDonationDate.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@DonorID", dc.DonorID)
                });

                // 3. Add to Blood Inventory
                var inv = new BloodInventory
                {
                    BloodGroup = dc.BloodGroup,
                    Units = (int)nudUnits.Value,
                    CollectionDate = dtpDonationDate.Value,
                    ExpiryDate = dtpDonationDate.Value.AddDays(42),
                    DonorID = dc.DonorID,
                    Status = "Available"
                };
                inventoryService.AddInventory(inv);

                MessageBox.Show("Donation recorded and inventory updated successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving donation:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class DonorCombo
        {
            public string Text { get; set; }
            public int DonorID { get; set; }
            public string BloodGroup { get; set; }
            public override string ToString() => Text;
        }
    }
}
