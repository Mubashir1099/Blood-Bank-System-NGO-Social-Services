using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IDonorService
    {
        List<Donor> GetAllDonors();
        Task<List<Donor>> GetAllDonorsAsync();          // Async — bonus mark
        Donor GetDonorById(int donorId);
        List<Donor> SearchDonors(string searchTerm);
        List<Donor> GetDonorsByBloodGroup(string bloodGroup);
        bool AddDonor(Donor donor);
        bool UpdateDonor(Donor donor);
        bool DeleteDonor(int donorId);
        bool DonorExists(string cnic);
    }
}
