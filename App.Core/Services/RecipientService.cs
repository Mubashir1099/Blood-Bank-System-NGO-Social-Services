using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using App.Core.Models;

namespace App.Core.Services
{
    public class RecipientService : IRecipientService
    {
        public List<Recipient> GetAllRecipients()
        {
            List<Recipient> list = new List<Recipient>();
            string query = "SELECT * FROM Recipients ORDER BY FullName";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                list.Add(MapRecipient(row));
            return list;
        }

        public Recipient GetRecipientById(int recipientId)
        {
            string query = "SELECT * FROM Recipients WHERE RecipientID = @RecipientID";
            var parameters = new SqlParameter[] { new SqlParameter("@RecipientID", recipientId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapRecipient(dt.Rows[0]);
            return null;
        }

        public List<Recipient> SearchRecipients(string searchTerm)
        {
            List<Recipient> list = new List<Recipient>();
            string query = @"SELECT * FROM Recipients WHERE 
                            FullName LIKE @Search OR CNIC LIKE @Search OR PhoneNumber LIKE @Search
                            ORDER BY FullName";
            var parameters = new SqlParameter[] { new SqlParameter("@Search", "%" + searchTerm + "%") };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                list.Add(MapRecipient(row));
            return list;
        }

        public bool AddRecipient(Recipient recipient)
        {
            string query = @"INSERT INTO Recipients (FullName, CNIC, BloodGroup, DateOfBirth, Gender,
                            PhoneNumber, Email, Address, HospitalName, MedicalCondition)
                            VALUES (@FullName, @CNIC, @BloodGroup, @DateOfBirth, @Gender,
                            @PhoneNumber, @Email, @Address, @HospitalName, @MedicalCondition)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@FullName", recipient.FullName),
                new SqlParameter("@CNIC", recipient.CNIC),
                new SqlParameter("@BloodGroup", recipient.BloodGroup),
                new SqlParameter("@DateOfBirth", recipient.DateOfBirth),
                new SqlParameter("@Gender", recipient.Gender),
                new SqlParameter("@PhoneNumber", recipient.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(recipient.Email) ? (object)DBNull.Value : recipient.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(recipient.Address) ? (object)DBNull.Value : recipient.Address),
                new SqlParameter("@HospitalName", string.IsNullOrEmpty(recipient.HospitalName) ? (object)DBNull.Value : recipient.HospitalName),
                new SqlParameter("@MedicalCondition", string.IsNullOrEmpty(recipient.MedicalCondition) ? (object)DBNull.Value : recipient.MedicalCondition),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateRecipient(Recipient recipient)
        {
            string query = @"UPDATE Recipients SET FullName=@FullName, CNIC=@CNIC, BloodGroup=@BloodGroup,
                            DateOfBirth=@DateOfBirth, Gender=@Gender, PhoneNumber=@PhoneNumber,
                            Email=@Email, Address=@Address, HospitalName=@HospitalName, MedicalCondition=@MedicalCondition
                            WHERE RecipientID=@RecipientID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RecipientID", recipient.RecipientID),
                new SqlParameter("@FullName", recipient.FullName),
                new SqlParameter("@CNIC", recipient.CNIC),
                new SqlParameter("@BloodGroup", recipient.BloodGroup),
                new SqlParameter("@DateOfBirth", recipient.DateOfBirth),
                new SqlParameter("@Gender", recipient.Gender),
                new SqlParameter("@PhoneNumber", recipient.PhoneNumber),
                new SqlParameter("@Email", string.IsNullOrEmpty(recipient.Email) ? (object)DBNull.Value : recipient.Email),
                new SqlParameter("@Address", string.IsNullOrEmpty(recipient.Address) ? (object)DBNull.Value : recipient.Address),
                new SqlParameter("@HospitalName", string.IsNullOrEmpty(recipient.HospitalName) ? (object)DBNull.Value : recipient.HospitalName),
                new SqlParameter("@MedicalCondition", string.IsNullOrEmpty(recipient.MedicalCondition) ? (object)DBNull.Value : recipient.MedicalCondition),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteRecipient(int recipientId)
        {
            string query = "DELETE FROM Recipients WHERE RecipientID = @RecipientID";
            var parameters = new SqlParameter[] { new SqlParameter("@RecipientID", recipientId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool RecipientExists(string cnic)
        {
            string query = "SELECT COUNT(*) FROM Recipients WHERE CNIC = @CNIC";
            var parameters = new SqlParameter[] { new SqlParameter("@CNIC", cnic) };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters)) > 0;
        }

        private Recipient MapRecipient(DataRow row)
        {
            return new Recipient
            {
                RecipientID = Convert.ToInt32(row["RecipientID"]),
                FullName = row["FullName"].ToString(),
                CNIC = row["CNIC"].ToString(),
                BloodGroup = row["BloodGroup"].ToString(),
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                Gender = row["Gender"].ToString(),
                PhoneNumber = row["PhoneNumber"].ToString(),
                Email = row["Email"] == DBNull.Value ? "" : row["Email"].ToString(),
                Address = row["Address"] == DBNull.Value ? "" : row["Address"].ToString(),
                HospitalName = row["HospitalName"] == DBNull.Value ? "" : row["HospitalName"].ToString(),
                MedicalCondition = row["MedicalCondition"] == DBNull.Value ? "" : row["MedicalCondition"].ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
