using System.Collections.Generic;
using Vacuum.Data.Models;

namespace Vacuum.Services.Interfaces;

public interface IFactionService
{
    FactionData? GetFaction(string factionId);
    List<FactionData> GetAllFactions();
    float GetStanding(string characterId, string factionId);
    void ModifyStanding(string characterId, string factionId, float delta, string reason);
    List<FactionStandingData> GetCharacterStandings(string characterId);
}
