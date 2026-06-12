using System;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using App.Core.Models;

namespace App.Core.Services
{
    public class UserService : IUserService
    {
        public User Login(string username, string password)
        {
            string query = "SELECT * FROM Users WHERE Username=@Username AND PasswordHash=@Password AND IsActive=1";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password)
            };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new User
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Role = row["Role"].ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };
            }
            return null;
        }

        public bool ChangePassword(int userId, string newPassword)
        {
            string query = "UPDATE Users SET PasswordHash=@Password WHERE UserID=@UserID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@Password", newPassword)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
