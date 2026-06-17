using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IVolunteerService
    {
        List<Volunteer> GetAllVolunteers();
        Task<List<Volunteer>> GetAllVolunteersAsync();
        Volunteer GetVolunteerById(int volunteerId);
        List<Volunteer> SearchVolunteers(string searchTerm);
        bool AddVolunteer(Volunteer volunteer);
        bool UpdateVolunteer(Volunteer volunteer);
        bool DeleteVolunteer(int volunteerId);
    }
}
