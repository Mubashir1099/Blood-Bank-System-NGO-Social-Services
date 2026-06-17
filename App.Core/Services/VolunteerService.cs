using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Services
{
    public class VolunteerService : IVolunteerService
    {
        public List<Volunteer> GetAllVolunteers()
        {
            List<Volunteer> volunteers = new List<Volunteer>();
            string query = "SELECT * FROM Volunteers WHERE IsActive = 1 ORDER BY FullName";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                volunteers.Add(MapVolunteer(row));
            return volunteers;
        }

        public async Task<List<Volunteer>> GetAllVolunteersAsync()
        {
            return await Task.Run(() => GetAllVolunteers());
        }

        public Volunteer GetVolunteerById(int volunteerId)
        {
            string query = "SELECT * FROM Volunteers WHERE VolunteerID = @VolunteerID";
            var parameters = new SqlParameter[] { new SqlParameter("@VolunteerID", volunteerId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapVolunteer(dt.Rows[0]);
            return null;
        }

        public List<Volunteer> SearchVolunteers(string searchTerm)
        {
            List<Volunteer> volunteers = new List<Volunteer>();
            string query = @"SELECT * FROM Volunteers WHERE IsActive = 1 AND 
                            (FullName LIKE @Search OR PhoneNumber LIKE @Search OR Skills LIKE @Search)
                            ORDER BY FullName";
            var parameters = new SqlParameter[] { new SqlParameter("@Search", "%" + searchTerm + "%") };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                volunteers.Add(MapVolunteer(row));
            return volunteers;
        }

        public bool AddVolunteer(Volunteer volunteer)
        {
            string query = @"INSERT INTO Volunteers (FullName, PhoneNumber, Email, Address, Skills, IsActive)
                            VALUES (@FullName, @PhoneNumber, @Email, @Address, @Skills, 1)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@FullName", volunteer.FullName),
                new SqlParameter("@PhoneNumber", volunteer.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(volunteer.Email) ? (object)DBNull.Value : volunteer.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(volunteer.Address) ? (object)DBNull.Value : volunteer.Address),
                new SqlParameter("@Skills", string.IsNullOrEmpty(volunteer.Skills) ? (object)DBNull.Value : volunteer.Skills)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateVolunteer(Volunteer volunteer)
        {
            string query = @"UPDATE Volunteers SET FullName=@FullName, PhoneNumber=@PhoneNumber, Email=@Email,
                            Address=@Address, Skills=@Skills
                            WHERE VolunteerID=@VolunteerID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@VolunteerID", volunteer.VolunteerID),
                new SqlParameter("@FullName", volunteer.FullName),
                new SqlParameter("@PhoneNumber", volunteer.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(volunteer.Email) ? (object)DBNull.Value : volunteer.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(volunteer.Address) ? (object)DBNull.Value : volunteer.Address),
                new SqlParameter("@Skills", string.IsNullOrEmpty(volunteer.Skills) ? (object)DBNull.Value : volunteer.Skills)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteVolunteer(int volunteerId)
        {
            // First remove from CampVolunteers association to avoid orphaned entries or integrity issues if there were cascades/etc,
            // though we don't have FK constraints that restrict delete, but it's clean to clean up associations.
            string cleanupQuery = "DELETE FROM CampVolunteers WHERE VolunteerID = @VolunteerID";
            var cleanupParams = new SqlParameter[] { new SqlParameter("@VolunteerID", volunteerId) };
            DatabaseHelper.ExecuteNonQuery(cleanupQuery, cleanupParams);

            string query = "UPDATE Volunteers SET IsActive = 0 WHERE VolunteerID = @VolunteerID";
            var parameters = new SqlParameter[] { new SqlParameter("@VolunteerID", volunteerId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        private Volunteer MapVolunteer(DataRow row)
        {
            return new Volunteer
            {
                VolunteerID = Convert.ToInt32(row["VolunteerID"]),
                FullName = row["FullName"].ToString() ?? string.Empty,
                PhoneNumber = row["PhoneNumber"].ToString() ?? string.Empty,
                Email = row["Email"] == DBNull.Value ? "" : row["Email"].ToString() ?? string.Empty,
                Address = row["Address"] == DBNull.Value ? "" : row["Address"].ToString() ?? string.Empty,
                Skills = row["Skills"] == DBNull.Value ? "" : row["Skills"].ToString() ?? string.Empty,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
