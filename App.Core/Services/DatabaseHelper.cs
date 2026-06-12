using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace App.Core.Services
{
    public static class DatabaseHelper
    {
        // *** Change this to match your SQL Server instance ***
        // Common values: .\SQLEXPRESS  |  (local)  |  localhost  |  DESKTOP-XYZ\SQLEXPRESS
        public static string ConnectionString =
     @"Data Source=localhost;Initial Catalog=BloodBankDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        public static bool TestConnection()
        {
            try { using (var c = GetConnection()) { c.Open(); return true; } }
            catch { return false; }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        using (var a = new SqlDataAdapter(cmd)) a.Fill(dt);
                    }
                }
            }
            catch (Exception ex) { throw new Exception("DB Error: " + ex.Message, ex); }
            return dt;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { throw new Exception("DB Error: " + ex.Message, ex); }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex) { throw new Exception("DB Error: " + ex.Message, ex); }
        }
    }
}
