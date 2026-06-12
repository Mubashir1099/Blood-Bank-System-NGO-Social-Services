using System.Collections.Generic;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IBloodRequestService
    {
        List<BloodRequest> GetAllRequests();
        BloodRequest GetRequestById(int requestId);
        List<BloodRequest> GetRequestsByStatus(string status);
        List<BloodRequest> GetRequestsByRecipient(int recipientId);
        bool AddRequest(BloodRequest request);
        bool UpdateRequest(BloodRequest request);
        bool ApproveRequest(int requestId, string approvedBy);
        bool RejectRequest(int requestId);
        bool DeleteRequest(int requestId);
    }
}
