using System;

namespace App.Core.Models
{
    public class Recipient
    {
        public int RecipientID { get; set; }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string HospitalName { get; set; }
        public string MedicalCondition { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
