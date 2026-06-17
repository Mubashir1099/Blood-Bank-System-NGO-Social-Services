using System;

namespace App.Core.Models
{
    public class Camp
    {
        public int CampID { get; set; }
        public string CampName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CampDate { get; set; }
        public int TargetUnits { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
