using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IShipService
{
    ShipData CreateShip(string characterId, string shipTypeId, string name);
    ShipData? GetShip(string shipId);
    List<ShipData> GetCharacterShips(string characterId);
    void UpdateShip(ShipData ship);
    void DeleteShip(string shipId);
    void FitModule(string shipId, string slotId, string moduleId);
    void RemoveModule(string shipId, string slotId);
    void RepairShip(string shipId);
}
