using System;

namespace App.Core.Models
{
    public class Donor
    {
        public int DonorID { get; set; }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public int Age => DateTime.Now.Year - DateOfBirth.Year;
    }
}
