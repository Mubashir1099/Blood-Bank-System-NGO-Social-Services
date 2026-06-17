using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Services
{
    public class CampService : ICampService
    {
        public List<Camp> GetAllCamps()
        {
            List<Camp> camps = new List<Camp>();
            string query = "SELECT * FROM Camps WHERE IsActive = 1 ORDER BY CampDate DESC";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                camps.Add(MapCamp(row));
            return camps;
        }

        public async Task<List<Camp>> GetAllCampsAsync()
        {
            return await Task.Run(() => GetAllCamps());
        }

        public Camp GetCampById(int campId)
        {
            string query = "SELECT * FROM Camps WHERE CampID = @CampID";
            var parameters = new SqlParameter[] { new SqlParameter("@CampID", campId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapCamp(dt.Rows[0]);
            return null;
        }

        public List<Camp> SearchCamps(string searchTerm)
        {
            List<Camp> camps = new List<Camp>();
            string query = @"SELECT * FROM Camps WHERE IsActive = 1 AND 
                            (CampName LIKE @Search OR Location LIKE @Search OR OrganizerName LIKE @Search)
                            ORDER BY CampDate DESC";
            var parameters = new SqlParameter[] { new SqlParameter("@Search", "%" + searchTerm + "%") };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                camps.Add(MapCamp(row));
            return camps;
        }

        public bool AddCamp(Camp camp)
        {
            string query = @"INSERT INTO Camps (CampName, Location, CampDate, TargetUnits, OrganizerName, IsActive)
                            VALUES (@CampName, @Location, @CampDate, @TargetUnits, @OrganizerName, 1)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CampName", camp.CampName),
                new SqlParameter("@Location", camp.Location),
                new SqlParameter("@CampDate", camp.CampDate),
                new SqlParameter("@TargetUnits", camp.TargetUnits),
                new SqlParameter("@OrganizerName", camp.OrganizerName)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateCamp(Camp camp)
        {
            string query = @"UPDATE Camps SET CampName=@CampName, Location=@Location, CampDate=@CampDate,
                            TargetUnits=@TargetUnits, OrganizerName=@OrganizerName
                            WHERE CampID=@CampID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CampID", camp.CampID),
                new SqlParameter("@CampName", camp.CampName),
                new SqlParameter("@Location", camp.Location),
                new SqlParameter("@CampDate", camp.CampDate),
                new SqlParameter("@TargetUnits", camp.TargetUnits),
                new SqlParameter("@OrganizerName", camp.OrganizerName)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteCamp(int campId)
        {
            string query = "UPDATE Camps SET IsActive = 0 WHERE CampID = @CampID";
            var parameters = new SqlParameter[] { new SqlParameter("@CampID", campId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public List<Volunteer> GetVolunteersForCamp(int campId)
        {
            List<Volunteer> volunteers = new List<Volunteer>();
            string query = @"SELECT v.* FROM Volunteers v
                            INNER JOIN CampVolunteers cv ON v.VolunteerID = cv.VolunteerID
                            WHERE cv.CampID = @CampID AND v.IsActive = 1
                            ORDER BY v.FullName";
            var parameters = new SqlParameter[] { new SqlParameter("@CampID", campId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
            {
                volunteers.Add(new Volunteer
                {
                    VolunteerID = Convert.ToInt32(row["VolunteerID"]),
                    FullName = row["FullName"].ToString() ?? string.Empty,
                    PhoneNumber = row["PhoneNumber"].ToString() ?? string.Empty,
                    Email = row["Email"] == DBNull.Value ? "" : row["Email"].ToString() ?? string.Empty,
                    Address = row["Address"] == DBNull.Value ? "" : row["Address"].ToString() ?? string.Empty,
                    Skills = row["Skills"] == DBNull.Value ? "" : row["Skills"].ToString() ?? string.Empty,
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                });
            }
            return volunteers;
        }

        public bool AssignVolunteerToCamp(int campId, int volunteerId)
        {
            // First check if already assigned
            string checkQuery = "SELECT COUNT(*) FROM CampVolunteers WHERE CampID = @CampID AND VolunteerID = @VolunteerID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CampID", campId),
                new SqlParameter("@VolunteerID", volunteerId)
            };
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, parameters));
            if (count > 0) return true; // Already assigned

            string insertQuery = "INSERT INTO CampVolunteers (CampID, VolunteerID) VALUES (@CampID, @VolunteerID)";
            var insertParams = new SqlParameter[]
            {
                new SqlParameter("@CampID", campId),
                new SqlParameter("@VolunteerID", volunteerId)
            };
            return DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams) > 0;
        }

        public bool RemoveVolunteerFromCamp(int campId, int volunteerId)
        {
            string query = "DELETE FROM CampVolunteers WHERE CampID = @CampID AND VolunteerID = @VolunteerID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@CampID", campId),
                new SqlParameter("@VolunteerID", volunteerId)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        private Camp MapCamp(DataRow row)
        {
            return new Camp
            {
                CampID = Convert.ToInt32(row["CampID"]),
                CampName = row["CampName"].ToString() ?? string.Empty,
                Location = row["Location"].ToString() ?? string.Empty,
                CampDate = Convert.ToDateTime(row["CampDate"]),
                TargetUnits = Convert.ToInt32(row["TargetUnits"]),
                OrganizerName = row["OrganizerName"].ToString() ?? string.Empty,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
