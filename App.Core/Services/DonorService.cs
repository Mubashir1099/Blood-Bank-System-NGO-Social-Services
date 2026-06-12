using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Services
{
    public class DonorService : IDonorService
    {
        public List<Donor> GetAllDonors()
        {
            List<Donor> donors = new List<Donor>();
            string query = "SELECT * FROM Donors WHERE IsActive = 1 ORDER BY FullName";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                donors.Add(MapDonor(row));
            return donors;
        }

        public async System.Threading.Tasks.Task<List<Donor>> GetAllDonorsAsync()
        {
            return await System.Threading.Tasks.Task.Run(() => GetAllDonors());
        }

        public Donor GetDonorById(int donorId)
        {
            string query = "SELECT * FROM Donors WHERE DonorID = @DonorID";
            var parameters = new SqlParameter[] { new SqlParameter("@DonorID", donorId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapDonor(dt.Rows[0]);
            return null;
        }

        public List<Donor> SearchDonors(string searchTerm)
        {
            List<Donor> donors = new List<Donor>();
            string query = @"SELECT * FROM Donors WHERE IsActive = 1 AND 
                            (FullName LIKE @Search OR CNIC LIKE @Search OR PhoneNumber LIKE @Search)
                            ORDER BY FullName";
            var parameters = new SqlParameter[] { new SqlParameter("@Search", "%" + searchTerm + "%") };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                donors.Add(MapDonor(row));
            return donors;
        }

        public List<Donor> GetDonorsByBloodGroup(string bloodGroup)
        {
            List<Donor> donors = new List<Donor>();
            string query = "SELECT * FROM Donors WHERE IsActive = 1 AND BloodGroup = @BloodGroup ORDER BY FullName";
            var parameters = new SqlParameter[] { new SqlParameter("@BloodGroup", bloodGroup) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                donors.Add(MapDonor(row));
            return donors;
        }

        public bool AddDonor(Donor donor)
        {
            string query = @"INSERT INTO Donors (FullName, CNIC, BloodGroup, DateOfBirth, Gender, 
                            PhoneNumber, Email, Address, LastDonationDate, IsActive)
                            VALUES (@FullName, @CNIC, @BloodGroup, @DateOfBirth, @Gender, 
                            @PhoneNumber, @Email, @Address, @LastDonationDate, 1)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@FullName", donor.FullName),
                new SqlParameter("@CNIC", donor.CNIC),
                new SqlParameter("@BloodGroup", donor.BloodGroup),
                new SqlParameter("@DateOfBirth", donor.DateOfBirth),
                new SqlParameter("@Gender", donor.Gender),
                new SqlParameter("@PhoneNumber", donor.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(donor.Email) ? (object)DBNull.Value : donor.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(donor.Address) ? (object)DBNull.Value : donor.Address),
                new SqlParameter("@LastDonationDate", donor.LastDonationDate.HasValue ? (object)donor.LastDonationDate.Value : DBNull.Value),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateDonor(Donor donor)
        {
            string query = @"UPDATE Donors SET FullName=@FullName, CNIC=@CNIC, BloodGroup=@BloodGroup,
                            DateOfBirth=@DateOfBirth, Gender=@Gender, PhoneNumber=@PhoneNumber,
                            Email=@Email, Address=@Address, LastDonationDate=@LastDonationDate
                            WHERE DonorID=@DonorID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@DonorID", donor.DonorID),
                new SqlParameter("@FullName", donor.FullName),
                new SqlParameter("@CNIC", donor.CNIC),
                new SqlParameter("@BloodGroup", donor.BloodGroup),
                new SqlParameter("@DateOfBirth", donor.DateOfBirth),
                new SqlParameter("@Gender", donor.Gender),
                new SqlParameter("@PhoneNumber", donor.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(donor.Email) ? (object)DBNull.Value : donor.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(donor.Address) ? (object)DBNull.Value : donor.Address),
                new SqlParameter("@LastDonationDate", donor.LastDonationDate.HasValue ? (object)donor.LastDonationDate.Value : DBNull.Value),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteDonor(int donorId)
        {
            // Soft delete
            string query = "UPDATE Donors SET IsActive = 0 WHERE DonorID = @DonorID";
            var parameters = new SqlParameter[] { new SqlParameter("@DonorID", donorId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DonorExists(string cnic)
        {
            string query = "SELECT COUNT(*) FROM Donors WHERE CNIC = @CNIC AND IsActive = 1";
            var parameters = new SqlParameter[] { new SqlParameter("@CNIC", cnic) };
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }

        private Donor MapDonor(DataRow row)
        {
            return new Donor
            {
                DonorID = Convert.ToInt32(row["DonorID"]),
                FullName = row["FullName"].ToString(),
                CNIC = row["CNIC"].ToString(),
                BloodGroup = row["BloodGroup"].ToString(),
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                Gender = row["Gender"].ToString(),
                PhoneNumber = row["PhoneNumber"].ToString(),
                Email = row["Email"] == DBNull.Value ? "" : row["Email"].ToString(),
                Address = row["Address"] == DBNull.Value ? "" : row["Address"].ToString(),
                LastDonationDate = row["LastDonationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["LastDonationDate"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
