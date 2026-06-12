using System;

namespace App.Core.Models
{
    public class BloodInventory
    {
        public int InventoryID { get; set; }
        public string BloodGroup { get; set; }
        public int Units { get; set; }
        public DateTime CollectionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? DonorID { get; set; }
        public string DonorName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsExpired => ExpiryDate < DateTime.Now;
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Now).Days;
    }
}
