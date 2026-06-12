using System;

namespace App.Core.Models
{
    public class BloodRequest
    {
        public int RequestID { get; set; }
        public int RecipientID { get; set; }
        public string RecipientName { get; set; }
        public string BloodGroup { get; set; }
        public int UnitsRequired { get; set; }
        public string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
