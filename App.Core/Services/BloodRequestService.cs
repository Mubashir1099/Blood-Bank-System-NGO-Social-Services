using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using App.Core.Models;

namespace App.Core.Services
{
    public class BloodRequestService : IBloodRequestService
    {
        public List<BloodRequest> GetAllRequests()
        {
            List<BloodRequest> list = new List<BloodRequest>();
            string query = @"SELECT br.*, r.FullName as RecipientName FROM BloodRequests br
                            JOIN Recipients r ON br.RecipientID = r.RecipientID
                            ORDER BY br.RequestDate DESC";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                list.Add(MapRequest(row));
            return list;
        }

        public BloodRequest GetRequestById(int requestId)
        {
            string query = @"SELECT br.*, r.FullName as RecipientName FROM BloodRequests br
                            JOIN Recipients r ON br.RecipientID = r.RecipientID
                            WHERE br.RequestID = @RequestID";
            var parameters = new SqlParameter[] { new SqlParameter("@RequestID", requestId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapRequest(dt.Rows[0]);
            return null;
        }

        public List<BloodRequest> GetRequestsByStatus(string status)
        {
            List<BloodRequest> list = new List<BloodRequest>();
            string query = @"SELECT br.*, r.FullName as RecipientName FROM BloodRequests br
                            JOIN Recipients r ON br.RecipientID = r.RecipientID
                            WHERE br.Status = @Status ORDER BY br.RequestDate DESC";
            var parameters = new SqlParameter[] { new SqlParameter("@Status", status) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                list.Add(MapRequest(row));
            return list;
        }

        public List<BloodRequest> GetRequestsByRecipient(int recipientId)
        {
            List<BloodRequest> list = new List<BloodRequest>();
            string query = @"SELECT br.*, r.FullName as RecipientName FROM BloodRequests br
                            JOIN Recipients r ON br.RecipientID = r.RecipientID
                            WHERE br.RecipientID = @RecipientID ORDER BY br.RequestDate DESC";
            var parameters = new SqlParameter[] { new SqlParameter("@RecipientID", recipientId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                list.Add(MapRequest(row));
            return list;
        }

        public bool AddRequest(BloodRequest request)
        {
            string query = @"INSERT INTO BloodRequests (RecipientID, BloodGroup, UnitsRequired, UrgencyLevel,
                            RequestDate, RequiredByDate, Status, Notes)
                            VALUES (@RecipientID, @BloodGroup, @UnitsRequired, @UrgencyLevel,
                            @RequestDate, @RequiredByDate, 'Pending', @Notes)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RecipientID", request.RecipientID),
                new SqlParameter("@BloodGroup", request.BloodGroup),
                new SqlParameter("@UnitsRequired", request.UnitsRequired),
                new SqlParameter("@UrgencyLevel", request.UrgencyLevel),
                new SqlParameter("@RequestDate", request.RequestDate == DateTime.MinValue ? DateTime.Now : request.RequestDate),
                new SqlParameter("@RequiredByDate", request.RequiredByDate.HasValue ? (object)request.RequiredByDate.Value : DBNull.Value),
                new SqlParameter("@Notes", string.IsNullOrEmpty(request.Notes) ? (object)DBNull.Value : request.Notes),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateRequest(BloodRequest request)
        {
            string query = @"UPDATE BloodRequests SET RecipientID=@RecipientID, BloodGroup=@BloodGroup,
                            UnitsRequired=@UnitsRequired, UrgencyLevel=@UrgencyLevel,
                            RequiredByDate=@RequiredByDate, Notes=@Notes
                            WHERE RequestID=@RequestID AND Status='Pending'";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RequestID", request.RequestID),
                new SqlParameter("@RecipientID", request.RecipientID),
                new SqlParameter("@BloodGroup", request.BloodGroup),
                new SqlParameter("@UnitsRequired", request.UnitsRequired),
                new SqlParameter("@UrgencyLevel", request.UrgencyLevel),
                new SqlParameter("@RequiredByDate", request.RequiredByDate.HasValue ? (object)request.RequiredByDate.Value : DBNull.Value),
                new SqlParameter("@Notes", string.IsNullOrEmpty(request.Notes) ? (object)DBNull.Value : request.Notes),
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool ApproveRequest(int requestId, string approvedBy)
        {
            string query = @"UPDATE BloodRequests SET Status='Approved', ApprovedBy=@ApprovedBy, ApprovalDate=GETDATE()
                            WHERE RequestID=@RequestID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RequestID", requestId),
                new SqlParameter("@ApprovedBy", approvedBy)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool RejectRequest(int requestId)
        {
            string query = "UPDATE BloodRequests SET Status='Rejected' WHERE RequestID=@RequestID";
            var parameters = new SqlParameter[] { new SqlParameter("@RequestID", requestId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteRequest(int requestId)
        {
            string query = "DELETE FROM BloodRequests WHERE RequestID = @RequestID";
            var parameters = new SqlParameter[] { new SqlParameter("@RequestID", requestId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        private BloodRequest MapRequest(DataRow row)
        {
            return new BloodRequest
            {
                RequestID = Convert.ToInt32(row["RequestID"]),
                RecipientID = Convert.ToInt32(row["RecipientID"]),
                RecipientName = row.Table.Columns.Contains("RecipientName") ? row["RecipientName"].ToString() : "",
                BloodGroup = row["BloodGroup"].ToString(),
                UnitsRequired = Convert.ToInt32(row["UnitsRequired"]),
                UrgencyLevel = row["UrgencyLevel"].ToString(),
                RequestDate = Convert.ToDateTime(row["RequestDate"]),
                RequiredByDate = row["RequiredByDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["RequiredByDate"]),
                Status = row["Status"].ToString(),
                Notes = row["Notes"] == DBNull.Value ? "" : row["Notes"].ToString(),
                ApprovedBy = row["ApprovedBy"] == DBNull.Value ? "" : row["ApprovedBy"].ToString(),
                ApprovalDate = row["ApprovalDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ApprovalDate"]),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
