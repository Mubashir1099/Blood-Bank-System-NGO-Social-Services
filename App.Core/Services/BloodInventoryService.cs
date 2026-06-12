using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using App.Core.Interfaces;
using App.Core.Models;

namespace App.Core.Services
{
    public class BloodInventoryService : IBloodInventoryService
    {
        public List<BloodInventory> GetAllInventory()
        {
            List<BloodInventory> list = new List<BloodInventory>();
            string query = @"SELECT bi.*, d.FullName as DonorName 
                            FROM BloodInventory bi
                            LEFT JOIN Donors d ON bi.DonorID = d.DonorID
                            ORDER BY bi.ExpiryDate";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                list.Add(MapInventory(row));
            return list;
        }

        public BloodInventory GetInventoryById(int inventoryId)
        {
            string query = @"SELECT bi.*, d.FullName as DonorName FROM BloodInventory bi
                            LEFT JOIN Donors d ON bi.DonorID = d.DonorID
                            WHERE bi.InventoryID = @InventoryID";
            var parameters = new SqlParameter[] { new SqlParameter("@InventoryID", inventoryId) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0) return MapInventory(dt.Rows[0]);
            return null;
        }

        public List<BloodInventory> GetInventoryByBloodGroup(string bloodGroup)
        {
            List<BloodInventory> list = new List<BloodInventory>();
            string query = @"SELECT bi.*, d.FullName as DonorName FROM BloodInventory bi
                            LEFT JOIN Donors d ON bi.DonorID = d.DonorID
                            WHERE bi.BloodGroup = @BloodGroup AND bi.Status = 'Available'
                            ORDER BY bi.ExpiryDate";
            var parameters = new SqlParameter[] { new SqlParameter("@BloodGroup", bloodGroup) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
            foreach (DataRow row in dt.Rows)
                list.Add(MapInventory(row));
            return list;
        }

        public Dictionary<string, int> GetBloodGroupSummary()
        {
            Dictionary<string, int> summary = new Dictionary<string, int>();
            string query = @"SELECT BloodGroup, SUM(Units) as TotalUnits 
                            FROM BloodInventory 
                            WHERE Status = 'Available' AND ExpiryDate >= GETDATE()
                            GROUP BY BloodGroup";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
                summary[row["BloodGroup"].ToString()] = Convert.ToInt32(row["TotalUnits"]);
            return summary;
        }

        public bool AddInventory(BloodInventory inventory)
        {
            string query = @"INSERT INTO BloodInventory (BloodGroup, Units, CollectionDate, ExpiryDate, DonorID, Status)
                            VALUES (@BloodGroup, @Units, @CollectionDate, @ExpiryDate, @DonorID, @Status)";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@BloodGroup", inventory.BloodGroup),
                new SqlParameter("@Units", inventory.Units),
                new SqlParameter("@CollectionDate", inventory.CollectionDate),
                new SqlParameter("@ExpiryDate", inventory.ExpiryDate),
                new SqlParameter("@DonorID", inventory.DonorID.HasValue ? (object)inventory.DonorID.Value : DBNull.Value),
                new SqlParameter("@Status", inventory.Status ?? "Available")
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateInventory(BloodInventory inventory)
        {
            string query = @"UPDATE BloodInventory SET BloodGroup=@BloodGroup, Units=@Units,
                            CollectionDate=@CollectionDate, ExpiryDate=@ExpiryDate,
                            DonorID=@DonorID, Status=@Status
                            WHERE InventoryID=@InventoryID";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@InventoryID", inventory.InventoryID),
                new SqlParameter("@BloodGroup", inventory.BloodGroup),
                new SqlParameter("@Units", inventory.Units),
                new SqlParameter("@CollectionDate", inventory.CollectionDate),
                new SqlParameter("@ExpiryDate", inventory.ExpiryDate),
                new SqlParameter("@DonorID", inventory.DonorID.HasValue ? (object)inventory.DonorID.Value : DBNull.Value),
                new SqlParameter("@Status", inventory.Status)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteInventory(int inventoryId)
        {
            string query = "DELETE FROM BloodInventory WHERE InventoryID = @InventoryID";
            var parameters = new SqlParameter[] { new SqlParameter("@InventoryID", inventoryId) };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeductUnits(string bloodGroup, int units)
        {
            // Deduct from oldest available stock first
            string query = @"UPDATE TOP(1) BloodInventory 
                            SET Units = Units - @Units,
                                Status = CASE WHEN Units - @Units <= 0 THEN 'Used' ELSE 'Available' END
                            WHERE BloodGroup = @BloodGroup AND Status = 'Available' 
                            AND ExpiryDate >= GETDATE() AND Units >= @Units";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@BloodGroup", bloodGroup),
                new SqlParameter("@Units", units)
            };
            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public int GetAvailableUnits(string bloodGroup)
        {
            string query = @"SELECT ISNULL(SUM(Units), 0) FROM BloodInventory 
                            WHERE BloodGroup = @BloodGroup AND Status = 'Available' AND ExpiryDate >= GETDATE()";
            var parameters = new SqlParameter[] { new SqlParameter("@BloodGroup", bloodGroup) };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        private BloodInventory MapInventory(DataRow row)
        {
            return new BloodInventory
            {
                InventoryID = Convert.ToInt32(row["InventoryID"]),
                BloodGroup = row["BloodGroup"].ToString(),
                Units = Convert.ToInt32(row["Units"]),
                CollectionDate = Convert.ToDateTime(row["CollectionDate"]),
                ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                DonorID = row["DonorID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DonorID"]),
                DonorName = row.Table.Columns.Contains("DonorName") && row["DonorName"] != DBNull.Value ? row["DonorName"].ToString() : "",
                Status = row["Status"].ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
