using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class ChangePasswordForm : Form
    {
        private readonly User currentUser;
        private readonly UserService userService = new UserService();

        private TextBox txtOldPwd, txtNewPwd, txtConfirm;
        private Button btnSave, btnCancel;
        private Label lblMsg;

        public ChangePasswordForm(User user)
        {
            currentUser = user;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Change Password";
            this.Size = new Size(420, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var titleBar = new Panel { Location = new Point(0, 0), Size = new Size(420, 52), BackColor = Color.FromArgb(180, 0, 0) };
            titleBar.Controls.Add(new Label { Text = "🔒  Change Password", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(14, 14) });

            int y = 70;
            AddField("Current Password", out txtOldPwd, y, true); y += 65;
            AddField("New Password", out txtNewPwd, y, true); y += 65;
            AddField("Confirm New Password", out txtConfirm, y, true); y += 65;

            lblMsg = new Label
            {
                Location = new Point(30, y),
                Size = new Size(340, 22),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Text = ""
            };
            y += 28;

            btnSave = new Button
            {
                Text = "✅  Update Password",
                Location = new Point(30, y), Size = new Size(180, 42),
                BackColor = Color.FromArgb(180, 0, 0), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(225, y), Size = new Size(100, 42),
                BackColor = Color.FromArgb(120, 120, 120), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] { titleBar, txtOldPwd, txtNewPwd, txtConfirm, lblMsg, btnSave, btnCancel });
            this.Height = y + 90;
        }

        private void AddField(string label, out TextBox txt, int y, bool isPassword)
        {
            this.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, AutoSize = true, Location = new Point(30, y) });
            txt = new TextBox { Location = new Point(30, y + 22), Size = new Size(340, 32), Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = isPassword };
            this.Controls.Add(txt);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            if (string.IsNullOrWhiteSpace(txtOldPwd.Text) || string.IsNullOrWhiteSpace(txtNewPwd.Text) || string.IsNullOrWhiteSpace(txtConfirm.Text))
            { lblMsg.Text = "All fields are required."; return; }

            // Verify current password
            var check = userService.Login(currentUser.Username, txtOldPwd.Text.Trim());
            if (check == null) { lblMsg.Text = "Current password is incorrect."; return; }

            if (txtNewPwd.Text.Trim().Length < 6) { lblMsg.Text = "New password must be at least 6 characters."; return; }
            if (txtNewPwd.Text.Trim() != txtConfirm.Text.Trim()) { lblMsg.Text = "New passwords do not match."; return; }

            if (userService.ChangePassword(currentUser.UserID, txtNewPwd.Text.Trim()))
            {
                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblMsg.Text = "Failed to update password.";
            }
        }
    }
}
