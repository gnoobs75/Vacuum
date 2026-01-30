using System.Collections.Generic;

namespace Vacuum.Services.Interfaces;

public interface ICombatService
{
    string InitiateCombat(string attackerId, string defenderId);
    void ApplyDamage(string targetShipId, float amount, string damageType);
    float CalculateDamage(string weaponModuleId, string targetShipId);
    void ResolveCombat(string combatId);
    List<CombatLogEntry> GetCombatLog(string combatId);
}

public class CombatLogEntry
{
    public string AttackerId { get; set; } = "";
    public string DefenderId { get; set; } = "";
    public float DamageDealt { get; set; }
    public string DamageType { get; set; } = "";
    public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;
}
