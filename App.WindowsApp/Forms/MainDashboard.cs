using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Core.Models;
using App.Core.Services;

namespace App.WindowsApp.Forms
{
    public class MainDashboard : Form
    {
        private readonly BloodInventoryService inventoryService = new BloodInventoryService();
        private readonly DonorService donorService = new DonorService();
        private readonly RecipientService recipientService = new RecipientService();
        private readonly BloodRequestService requestService = new BloodRequestService();
        private readonly CampService campService = new CampService();
        private readonly VolunteerService volunteerService = new VolunteerService();

        private Panel pnlHeader, pnlSidebar, pnlContent, pnlFooter;
        private Button btnNavDashboard, btnNavDonors, btnNavDonation, btnNavInventory;
        private Button btnNavCharts;
        private Button btnNavRecipients, btnNavRequests, btnNavReports, btnNavChangePwd, btnNavLogout;
        private Button btnNavCamps, btnNavVolunteers;
        private Button activeNavBtn;
        private Label lblUser, lblDateTime;
        private System.Windows.Forms.Timer clockTimer;
        private readonly User currentUser;

        public MainDashboard(User user)
        {
            currentUser = user;
            InitializeLayout();
            StartClock();
            ShowDashboard();
        }

        private void InitializeLayout()
        {
            this.Text = "Blood Bank Management System";
            this.Size = new Size(1280, 780);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(243, 244, 248);
            this.WindowState = FormWindowState.Maximized;
            BuildHeader(); BuildSidebar(); BuildContent(); BuildFooter();
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(180, 0, 0) };
            pnlHeader.Controls.Add(new Label { Text = "   Blood Bank Management System", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(6, 14) });
            lblDateTime = new Label { Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(255, 210, 210), AutoSize = true, Location = new Point(900, 10) };
            lblUser = new Label { Text = $"  {currentUser.FullName}   [{currentUser.Role}]", Font = new Font("Segoe UI", 9.5f), ForeColor = Color.White, AutoSize = true, Location = new Point(900, 30) };
            pnlHeader.Controls.AddRange(new Control[] { lblDateTime, lblUser });
            this.Resize += (s, e) => { lblDateTime.Left = pnlHeader.Width - lblDateTime.Width - 20; lblUser.Left = pnlHeader.Width - lblUser.Width - 20; };
        }

        private void BuildSidebar()
        {
            pnlSidebar = new Panel { Width = 215, Dock = DockStyle.Left, BackColor = Color.FromArgb(22, 27, 38) };
            var lblNav = new Label { Text = "NAVIGATION", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(100, 110, 130), AutoSize = true, Location = new Point(18, 18) };
            var sep1 = new Panel { Location = new Point(10, 36), Size = new Size(195, 1), BackColor = Color.FromArgb(40, 45, 60) };

            btnNavDashboard  = NavBtn("  Dashboard",        50);
            btnNavDonors     = NavBtn("  Donors",           108);
            btnNavDonation   = NavBtn("  Record Donation",  166);
            btnNavInventory  = NavBtn("  Blood Inventory",  224);
            btnNavRecipients = NavBtn("  Recipients",       282);
            btnNavRequests   = NavBtn("  Blood Requests",   340);
            btnNavCamps      = NavBtn("  NGO Camps",        398);
            btnNavVolunteers = NavBtn("  Volunteers",       456);
            btnNavReports    = NavBtn("  Reports",          514);
            btnNavCharts     = NavBtn("  Charts",           572);

            var sep2 = new Panel { Location = new Point(10, 630), Size = new Size(195, 1), BackColor = Color.FromArgb(40, 45, 60) };
            btnNavChangePwd = NavBtn("  Change Password",  640);
            btnNavLogout    = NavBtn("  Logout",           698);
            btnNavLogout.ForeColor = Color.FromArgb(255, 110, 110);

            btnNavDashboard.Click  += (s, e) => { SetNav(btnNavDashboard);  ShowDashboard(); };
            btnNavDonors.Click     += (s, e) => { SetNav(btnNavDonors);     ShowSection(new DonorManagementControl()); };
            btnNavDonation.Click   += (s, e) => { SetNav(btnNavDonation);   OpenDonationForm(); };
            btnNavInventory.Click  += (s, e) => { SetNav(btnNavInventory);  ShowSection(new BloodInventoryControl()); };
            btnNavRecipients.Click += (s, e) => { SetNav(btnNavRecipients); ShowSection(new RecipientManagementControl()); };
            btnNavRequests.Click   += (s, e) => { SetNav(btnNavRequests);   ShowSection(new BloodRequestControl(currentUser)); };
            btnNavCamps.Click      += (s, e) => { SetNav(btnNavCamps);      ShowSection(new CampManagementControl()); };
            btnNavVolunteers.Click += (s, e) => { SetNav(btnNavVolunteers); ShowSection(new VolunteerManagementControl()); };
            btnNavReports.Click    += (s, e) => { SetNav(btnNavReports);    new ReportForm().ShowDialog(this); };
            btnNavCharts.Click     += (s, e) => { SetNav(btnNavCharts);     new ChartForm().ShowDialog(this); };
            btnNavChangePwd.Click  += (s, e) => { new ChangePasswordForm(currentUser).ShowDialog(this); };
            btnNavLogout.Click     += BtnLogout_Click;

            pnlSidebar.Controls.AddRange(new Control[] { lblNav, sep1, btnNavDashboard, btnNavDonors, btnNavDonation, btnNavInventory, btnNavRecipients, btnNavRequests, btnNavCamps, btnNavVolunteers, btnNavReports, btnNavCharts, sep2, btnNavChangePwd, btnNavLogout });
        }

        private Button NavBtn(string text, int y)
        {
            var b = new Button { Text = text, Size = new Size(215, 50), Location = new Point(0, y), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(185, 195, 210), BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 42, 58);
            return b;
        }

        private void SetNav(Button btn)
        {
            if (activeNavBtn != null) { activeNavBtn.BackColor = Color.Transparent; activeNavBtn.ForeColor = Color.FromArgb(185, 195, 210); }
            activeNavBtn = btn;
            btn.BackColor = Color.FromArgb(180, 0, 0);
            btn.ForeColor = Color.White;
        }

        private void BuildContent()
        {
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(243, 244, 248), Padding = new Padding(22, 18, 22, 18) };
        }

        private void BuildFooter()
        {
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(22, 27, 38) };
            pnlFooter.Controls.Add(new Label { Text = "   Blood Bank Management System  |  Advanced Programming (COSC-5136)  |  Spring 2026", Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(100, 110, 130), AutoSize = true, Location = new Point(0, 7) });
        }

        private void StartClock()
        {
            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => { lblDateTime.Text = DateTime.Now.ToString("dddd, dd MMM yyyy   hh:mm:ss tt"); lblDateTime.Left = pnlHeader.Width - lblDateTime.Width - 20; };
            clockTimer.Start();
        }

        private void ShowSection(UserControl uc) { pnlContent.Controls.Clear(); uc.Dock = DockStyle.Fill; pnlContent.Controls.Add(uc); }

        private void OpenDonationForm()
        {
            new DonationForm().ShowDialog(this);
            if (activeNavBtn == btnNavInventory) ShowSection(new BloodInventoryControl());
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            { clockTimer.Stop(); Application.Restart(); }
        }

        private void ShowDashboard()
        {
            SetNav(btnNavDashboard);
            pnlContent.Controls.Clear();
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            pnlContent.Controls.Add(scroll);

            scroll.Controls.Add(new Label { Text = $"Good {GetGreeting()}, {currentUser.FullName}!", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 40), AutoSize = true, Location = new Point(0, 0) });
            scroll.Controls.Add(new Label { Text = "Here is your Blood Bank summary.", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 36) });

            try
            {
                var donors     = donorService.GetAllDonors();
                var recipients = recipientService.GetAllRecipients();
                var summary    = inventoryService.GetBloodGroupSummary();
                int totalUnits = 0; foreach (var v in summary.Values) totalUnits += v;
                int pending    = requestService.GetRequestsByStatus("Pending").Count;
                int approved   = requestService.GetRequestsByStatus("Approved").Count;
                
                var camps      = campService.GetAllCamps();
                int activeCamps = camps.FindAll(c => c.CampDate.Date >= DateTime.Today).Count;
                var volunteers = volunteerService.GetAllVolunteers();

                int xc = 0, yc = 76;
                // Row 1 Cards
                AddCard(scroll, "Total Donors",    donors.Count.ToString(),    Color.FromArgb(220,53,69),  xc,     yc, () => { SetNav(btnNavDonors);     ShowSection(new DonorManagementControl()); });
                AddCard(scroll, "Recipients",      recipients.Count.ToString(), Color.FromArgb(13,110,253), xc+188, yc, () => { SetNav(btnNavRecipients); ShowSection(new RecipientManagementControl()); });
                AddCard(scroll, "Blood Units",     totalUnits.ToString(),       Color.FromArgb(25,135,84),  xc+376, yc, () => { SetNav(btnNavInventory);  ShowSection(new BloodInventoryControl()); });
                AddCard(scroll, "Pending Req.",    pending.ToString(),          Color.FromArgb(255,153,0),  xc+564, yc, () => { SetNav(btnNavRequests);   ShowSection(new BloodRequestControl(currentUser)); });
                AddCard(scroll, "Approved Req.",   approved.ToString(),         Color.FromArgb(32,201,151), xc+752, yc, () => { SetNav(btnNavRequests);   ShowSection(new BloodRequestControl(currentUser)); });

                // Row 2 Cards (NGO & Services Stats)
                int yc2 = yc + 128;
                AddCard(scroll, "Active Camps",    activeCamps.ToString(),      Color.FromArgb(111,66,193), xc,     yc2, () => { SetNav(btnNavCamps);      ShowSection(new CampManagementControl()); });
                AddCard(scroll, "Volunteers",      volunteers.Count.ToString(), Color.FromArgb(25,135,84),  xc+188, yc2, () => { SetNav(btnNavVolunteers); ShowSection(new VolunteerManagementControl()); });

                int yBG = yc2 + 128;
                scroll.Controls.Add(SectionLbl("Blood Inventory by Group", 0, yBG)); yBG += 38;
                string[] bgs = {"A+","A-","B+","B-","O+","O-","AB+","AB-"};
                int xBG = 0;
                foreach (var bg in bgs)
                {
                    int u = summary.ContainsKey(bg) ? summary[bg] : 0;
                    Color c = u == 0 ? Color.Gray : u < 3 ? Color.FromArgb(220,53,69) : Color.FromArgb(25,135,84);
                    AddBGCard(scroll, bg, u, c, xBG, yBG);
                    xBG += 118;
                }

                int yQA = yBG + 110;
                scroll.Controls.Add(SectionLbl("Quick Actions", 0, yQA)); yQA += 38;
                AddQA(scroll, "  Add Donor",         Color.FromArgb(220,53,69),  0,   yQA, () => { SetNav(btnNavDonors);     ShowSection(new DonorManagementControl()); });
                AddQA(scroll, "  Record Donation",   Color.FromArgb(13,110,253), 178, yQA, OpenDonationForm);
                AddQA(scroll, "  New Blood Request", Color.FromArgb(255,153,0),  356, yQA, () => { SetNav(btnNavRequests);   ShowSection(new BloodRequestControl(currentUser)); });
                AddQA(scroll, "  View Reports",      Color.FromArgb(32,201,151), 534, yQA, () => { SetNav(btnNavReports);   new ReportForm().ShowDialog(this); });
                AddQA(scroll, "  View Charts",       Color.FromArgb(111,66,193), 712, yQA, () => { SetNav(btnNavCharts);    new ChartForm().ShowDialog(this); });

                // Low stock alert
                var low = new System.Text.StringBuilder();
                foreach (var bg in bgs) if (!summary.ContainsKey(bg) || summary[bg] < 3) low.Append(bg + "  ");
                if (low.Length > 0)
                {
                    int yA = yQA + 72;
                    var ap = new Panel { Location = new Point(0, yA), Size = new Size(960, 48), BackColor = Color.FromArgb(255,243,205), BorderStyle = BorderStyle.FixedSingle };
                    ap.Controls.Add(new Label { Text = $"  WARNING — Low stock blood groups: {low}  Please arrange donations urgently.", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(133,77,14), AutoSize = false, Size = new Size(950, 46), Location = new Point(0,0), TextAlign = ContentAlignment.MiddleLeft });
                    scroll.Controls.Add(ap);
                }
            }
            catch (Exception ex)
            {
                scroll.Controls.Add(new Label { Text = "Could not load data: " + ex.Message + "\n\nCheck database connection.", Font = new Font("Segoe UI", 10), ForeColor = Color.Red, AutoSize = true, Location = new Point(0, 80) });
            }
        }

        private string GetGreeting() { int h = DateTime.Now.Hour; if (h < 12) return "Morning"; if (h < 17) return "Afternoon"; return "Evening"; }
        private Label SectionLbl(string t, int x, int y) => new Label { Text = t, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 40), AutoSize = true, Location = new Point(x, y) };

        private void AddCard(Panel p, string title, string value, Color accent, int x, int y, Action onClick)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(176, 112), BackColor = Color.White, Cursor = Cursors.Hand };
            card.Controls.Add(new Panel { Location = new Point(0,0), Size = new Size(5,112), BackColor = accent });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = accent, AutoSize = true, Location = new Point(12, 16) });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(12, 84) });
            card.Click += (s, e) => onClick?.Invoke();
            foreach (Control c in card.Controls) { c.Click += (s, e) => onClick?.Invoke(); c.Cursor = Cursors.Hand; }
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(252, 246, 246);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;
            p.Controls.Add(card);
        }

        private void AddBGCard(Panel p, string bg, int units, Color color, int x, int y)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(106, 88), BackColor = Color.White };
            card.Controls.Add(new Panel { Location = new Point(0,0), Size = new Size(106,5), BackColor = color });
            card.Controls.Add(new Label { Text = bg, Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = color, AutoSize = true, Location = new Point(14,10) });
            card.Controls.Add(new Label { Text = $"{units} units", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray, AutoSize = true, Location = new Point(10,60) });
            p.Controls.Add(card);
        }

        private void AddQA(Panel p, string text, Color color, int x, int y, Action onClick)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(166, 46), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => onClick?.Invoke();
            p.Controls.Add(b);
        }
    }
}
