using System;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class LoginForm : Form
    {
        private Panel leftPanel;
        private Panel rightPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblBloodDrop;
        private Label lblWelcome;
        private Label lblSignIn;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblError;
        private CheckBox chkShowPassword;

        private UserService userService = new UserService();
        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Blood Bank Management System - Login";
            this.Size = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Left Panel (Red gradient background)
            leftPanel = new Panel
            {
                Size = new Size(380, 560),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(180, 0, 0)
            };

            lblBloodDrop = new Label
            {
                Text = "🩸",
                Font = new Font("Segoe UI Emoji", 64),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(130, 80)
            };

            lblTitle = new Label
            {
                Text = "Blood Bank",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(70, 200)
            };

            lblSubtitle = new Label
            {
                Text = "Management System",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.FromArgb(255, 200, 200),
                AutoSize = true,
                Location = new Point(60, 250)
            };

            var lblTagline = new Label
            {
                Text = "\"Saving Lives, One Drop at a Time\"",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 220, 220),
                AutoSize = false,
                Size = new Size(340, 40),
                Location = new Point(20, 380),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblVersion = new Label
            {
                Text = "Version 1.0 | Advanced Programming (.NET)",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(255, 180, 180),
                AutoSize = false,
                Size = new Size(380, 25),
                Location = new Point(0, 500),
                TextAlign = ContentAlignment.MiddleCenter
            };

            leftPanel.Controls.AddRange(new Control[] { lblBloodDrop, lblTitle, lblSubtitle, lblTagline, lblVersion });

            // Right Panel
            rightPanel = new Panel
            {
                Size = new Size(520, 560),
                Location = new Point(380, 0),
                BackColor = Color.White
            };

            lblWelcome = new Label
            {
                Text = "Welcome Back!",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(80, 80)
            };

            lblSignIn = new Label
            {
                Text = "Please sign in to continue",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(80, 125)
            };

            // Divider
            var divider = new Panel
            {
                Size = new Size(360, 2),
                Location = new Point(80, 155),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            lblUsername = new Label
            {
                Text = "USERNAME",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(80, 190)
            };

            txtUsername = new TextBox
            {
                Size = new Size(360, 38),
                Location = new Point(80, 213),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(5)
            };

            lblPassword = new Label
            {
                Text = "PASSWORD",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(80, 275)
            };

            txtPassword = new TextBox
            {
                Size = new Size(360, 38),
                Location = new Point(80, 298),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 248, 248),
                UseSystemPasswordChar = true
            };

            chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                Location = new Point(80, 345),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            chkShowPassword.CheckedChanged += (s, e) => {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            };

            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(80, 375)
            };

            btnLogin = new Button
            {
                Text = "SIGN IN",
                Size = new Size(360, 48),
                Location = new Point(80, 395),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            var lblHint = new Label
            {
                Text = "Default: admin / Admin123",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true,
                Location = new Point(160, 455)
            };

            rightPanel.Controls.AddRange(new Control[] {
                lblWelcome, lblSignIn, divider, lblUsername, txtUsername,
                lblPassword, txtPassword, chkShowPassword, lblError, btnLogin, lblHint
            });

            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);

            this.AcceptButton = btnLogin;
            txtUsername.Text = "admin";
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Please enter username and password.";
                return;
            }

            try
            {
                User user = userService.Login(txtUsername.Text.Trim(), txtPassword.Text.Trim());
                if (user != null)
                {
                    LoggedInUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblError.Text = "Invalid username or password. Please try again.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message + "\n\nPlease check your database connection in DatabaseHelper.cs",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
