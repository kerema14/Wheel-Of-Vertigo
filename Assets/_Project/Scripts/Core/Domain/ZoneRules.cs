using System;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>
    /// The game's progression rules as plain C#: no Unity types, no assets.
    /// Easy to unit test, e.g. new ZoneRules(5, 30, 0.1f).GetZoneType(30) == ZoneType.Super.
    /// </summary>
    public sealed class ZoneRules
    {
        public const int FirstZone = 1;

        private readonly int _safeInterval;
        private readonly int _superInterval;
        private readonly float _growthPerZone;

        public ZoneRules(int safeInterval, int superInterval, float growthPerZone)
        {
            if (safeInterval < 2) throw new ArgumentOutOfRangeException(nameof(safeInterval));
            if (superInterval < 2) throw new ArgumentOutOfRangeException(nameof(superInterval));
            if (growthPerZone < 0f) throw new ArgumentOutOfRangeException(nameof(growthPerZone));
            if (safeInterval % superInterval == 0)
                throw new ArgumentException("Every safe zone would also be a super zone.", nameof(safeInterval));

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

        // "Rewards that get better every zone."
        public int ScaleAmount(int baseAmount, int zone) =>
            (int)Math.Round(baseAmount * (1f + _growthPerZone * (zone - FirstZone)));

        /// <summary>The first zone at or after <paramref name="fromZone"/> with the given type.</summary>
        public int NextZoneOfType(int fromZone, ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Super:
                    return RoundUpToMultiple(fromZone, _superInterval);
                case ZoneType.Safe:
                    // A super zone is never also a safe zone, so step past any multiple of the super interval.
                    int zone = RoundUpToMultiple(fromZone, _safeInterval);
                    while (GetZoneType(zone) != ZoneType.Safe) zone += _safeInterval;
                    return zone;
                default:
                    int normal = Math.Max(fromZone, FirstZone);
                    while (GetZoneType(normal) != ZoneType.Normal) normal++;
                    return normal;
            }
        }

        private static int RoundUpToMultiple(int value, int multiple)
        {
            int clamped = Math.Max(value, FirstZone);
            return (clamped + multiple - 1) / multiple * multiple;
        }
    }
}
