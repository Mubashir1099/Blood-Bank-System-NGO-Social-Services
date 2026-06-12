using System.Collections.Generic;
using App.Core.Models;

namespace App.Core.Interfaces
{
    public interface IBloodInventoryService
    {
        List<BloodInventory> GetAllInventory();
        BloodInventory GetInventoryById(int inventoryId);
        List<BloodInventory> GetInventoryByBloodGroup(string bloodGroup);
        Dictionary<string, int> GetBloodGroupSummary();
        bool AddInventory(BloodInventory inventory);
        bool UpdateInventory(BloodInventory inventory);
        bool DeleteInventory(int inventoryId);
        bool DeductUnits(string bloodGroup, int units);
        int GetAvailableUnits(string bloodGroup);
    }
}
