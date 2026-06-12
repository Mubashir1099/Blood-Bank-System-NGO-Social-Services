using System;
using System.Windows.Forms;
using App.Core.Services;
using App.WindowsApp.Forms;

namespace App.WindowsApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Test DB connection first
            if (!DatabaseHelper.TestConnection())
            {
                var result = MessageBox.Show(
                    "Cannot connect to database!\n\n" +
                    "Please make sure:\n" +
                    "1. SQL Server / SQL Express is running\n" +
                    "2. Database 'BloodBankDB' exists\n" +
                    "3. Connection string in DatabaseHelper.cs is correct\n\n" +
                    "Current connection string:\n" + DatabaseHelper.ConnectionString +
                    "\n\nClick YES to continue anyway (demo mode) or NO to exit.",
                    "Database Connection Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.No) return;
            }

            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
            {
                Application.Run(new MainDashboard(loginForm.LoggedInUser));
            }
        }
    }
}
