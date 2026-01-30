namespace Vacuum.Data.Enums;

public enum SlotType
{
    High,
    Mid,
    Low,
    Rig
}

public enum DamageType
{
    Kinetic,
    Thermal,
    Electromagnetic,
    Explosive
}

public enum ShipClass
{
    Frigate,
    Destroyer,
    Cruiser,
    Battlecruiser,
    Battleship,
    Capital
}

public enum TechLevel
{
    Tech1,
    Tech2,
    Faction
}

public enum SecurityLevel
{
    HighSec,
    LowSec,
    NullSec,
    WormholeSpace
}

public enum PodStatus
{
    Active,
    Destroyed,
    Cloned
}

public enum TutorialDifficulty
{
    Beginner,
    Intermediate,
    Advanced
}

public enum QualityLevel
{
    Low,
    Medium,
    High,
    Ultra
}

public enum ColorblindMode
{
    None,
    Protanopia,
    Deuteranopia,
    Tritanopia
}

public enum WormholeClassification
{
    Stable,
    Unstable,
    Critical,
    Collapsing
}

// CorporationRole moved to CorporationEnums.cs

public enum WarpState
{
    Idle,
    Aligning,
    Accelerating,
    InWarp,
    Decelerating,
    Exiting
}
