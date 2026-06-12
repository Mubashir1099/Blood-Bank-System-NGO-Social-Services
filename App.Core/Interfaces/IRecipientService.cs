using System.Collections.Generic;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IRecipientService
    {
        List<Recipient> GetAllRecipients();
        Recipient GetRecipientById(int recipientId);
        List<Recipient> SearchRecipients(string searchTerm);
        bool AddRecipient(Recipient recipient);
        bool UpdateRecipient(Recipient recipient);
        bool DeleteRecipient(int recipientId);
        bool RecipientExists(string cnic);
    }
}
