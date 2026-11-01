using System;
using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>
    /// The game's progression rules: which zone type each zone is, what reaching it unlocks, and how rewards
    /// grow. Zone types are data (<see cref="ZoneDefinition"/>), so adding one needs no change here.
    /// </summary>
    public sealed class ZoneRules
    {
        public const int FirstZone = 1;

        private readonly IReadOnlyList<ZoneDefinition> _zones;
        private readonly float _growthPerZone;

        public ZoneDefinition DefaultZone { get; }

        /// <param name="zones">Special zone types. A zone takes the first one whose interval divides it.</param>
        /// <param name="defaultZone">The type of every zone that no special type claims.</param>
        public ZoneRules(IReadOnlyList<ZoneDefinition> zones, ZoneDefinition defaultZone, float growthPerZone)
        {
            _zones = zones ?? throw new ArgumentNullException(nameof(zones));
            DefaultZone = defaultZone != null ? defaultZone : throw new ArgumentNullException(nameof(defaultZone));
            if (growthPerZone < 0f) throw new ArgumentOutOfRangeException(nameof(growthPerZone));
            _growthPerZone = growthPerZone;

            for (int i = 0; i < _zones.Count; i++)
            {
                ZoneDefinition zone = _zones[i];
                if (zone == null) throw new ArgumentException($"Zone type {i} is empty.", nameof(zones));
                if (zone.Interval < 2) throw new ArgumentException($"{zone.name} would claim every zone.", nameof(zones));

                // If an earlier interval divides this one, every zone of this type is claimed first by the
                // earlier type and this one never appears. Ruling that out also keeps NextZoneOf finite.
                for (int j = 0; j < i; j++)
                    if (zone.Interval % _zones[j].Interval == 0)
                        throw new ArgumentException($"Every {zone.name} zone would be a {_zones[j].name} zone.", nameof(zones));
            }
        }

        public ZoneDefinition GetZone(int zone)
        {
            foreach (ZoneDefinition definition in _zones)
                if (zone % definition.Interval == 0)
                    return definition;

            return DefaultZone;
        }

        /// <summary>1 for the first zone of its type, 2 for the second and so on.</summary>
        public int GetOrdinal(int zone)
        {
            ZoneDefinition definition = GetZone(zone);
            int step = definition == DefaultZone ? 1 : definition.Interval;
            int count = 0;

            for (int earlier = step; earlier <= zone; earlier += step)
                if (GetZone(earlier) == definition)
                    count++;

            return count;
        }

        /// <summary>The reward guaranteed for reaching <paramref name="zone"/>, or null.</summary>
        public RewardDefinition GetUnlock(int zone)
        {
            IReadOnlyList<RewardDefinition> unlocks = GetZone(zone).Unlocks;
            if (unlocks.Count == 0) return null;

            return unlocks[(GetOrdinal(zone) - 1) % unlocks.Count];
        }

        /// <summary>The first zone at or after <paramref name="fromZone"/> of the given type.</summary>
        public int NextZoneOf(int fromZone, ZoneDefinition definition)
        {
            int zone = Math.Max(fromZone, FirstZone);
            int step = 1;

            if (definition != DefaultZone)
            {
                if (!Contains(definition))
                    throw new ArgumentException($"{(definition != null ? definition.name : "null")} isn't one of the zone types.", nameof(definition));

                step = definition.Interval;
                zone = (zone + step - 1) / step * step;
            }

            while (GetZone(zone) != definition)
                zone += step;

            return zone;
        }

        private bool Contains(ZoneDefinition definition)
        {
            foreach (ZoneDefinition zone in _zones)
                if (zone == definition)
                    return true;

            return false;
        }

        // "Rewards that get better every zone."
        public int ScaleAmount(int baseAmount, int zone) =>
            (int)Math.Round(baseAmount * (1f + _growthPerZone * (zone - FirstZone)));
    }
}
