using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface ICampService
    {
        List<Camp> GetAllCamps();
        Task<List<Camp>> GetAllCampsAsync();
        Camp GetCampById(int campId);
        List<Camp> SearchCamps(string searchTerm);
        bool AddCamp(Camp camp);
        bool UpdateCamp(Camp camp);
        bool DeleteCamp(int campId);
        List<Volunteer> GetVolunteersForCamp(int campId);
        bool AssignVolunteerToCamp(int campId, int volunteerId);
        bool RemoveVolunteerFromCamp(int campId, int volunteerId);
    }
}
