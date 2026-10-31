using System;



/// <summary>
/// The game's progression rules as plain C#: no Unity types, no assets.
/// Easy to unit test, e.g. new ZoneRules(5, 30, 0.1f).GetZoneType(30) == ZoneType.Super.
/// </summary>
public class ZoneRules
{
    private readonly int _safeInterval;
    private readonly int _superInterval;
    private readonly float _growthPerZone;

    public ZoneRules(int safeInterval, int superInterval, float growthPerZone)
    {
        _safeInterval = safeInterval;
        _superInterval = superInterval;
        _growthPerZone = growthPerZone;
    }

    public ZoneType GetZoneType(int zone)
    {
        if (zone % _superInterval == 0) return ZoneType.Super;   // check first: 30 is also a multiple of 5
        if (zone % _safeInterval == 0) return ZoneType.Safe;
        return ZoneType.Normal;
    }

    // "Player can choose to leave ... when the zone is safe or at the super zone."
    // Whether the wheel is spinning is the presenter's concern, not a zone rule.
    public bool CanLeaveAt(int zone) => GetZoneType(zone) != ZoneType.Normal;

    // "Rewards that get better every zone."
    public int ScaleAmount(int baseAmount, int zone) =>
        (int)Math.Round(baseAmount * (1f + _growthPerZone * (zone - 1)));
}