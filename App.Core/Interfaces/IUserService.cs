using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IUserService
    {
        User Login(string username, string password);
        bool ChangePassword(int userId, string newPassword);
    }
}
