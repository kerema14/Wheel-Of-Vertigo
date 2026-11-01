using System;
using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>Says which reward a zone unlocks just for reaching it. Tests supply their own.</summary>
    public interface ISuperZoneRewards
    {
        /// <returns>The guaranteed reward for <paramref name="zone"/>, or null if the zone has none.</returns>
        RewardDefinition GetReward(int zone);
    }

    /// <summary>
    /// Hands out a configured list of rewards across the super zones in order: the first super zone gets the
    /// first reward, the second gets the second, and the list starts over once every reward has been used.
    /// </summary>
    public sealed class SuperZoneRewardTable : ISuperZoneRewards
    {
        private readonly IReadOnlyList<RewardDefinition> _rewards;
        private readonly ZoneRules _rules;

        public SuperZoneRewardTable(IReadOnlyList<RewardDefinition> rewards, ZoneRules rules)
        {
            _rewards = rewards ?? Array.Empty<RewardDefinition>();
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public RewardDefinition GetReward(int zone)
        {
            int number = _rules.GetSuperZoneNumber(zone);
            if (number == 0 || _rewards.Count == 0) return null;

            return _rewards[(number - 1) % _rewards.Count];
        }
    }
}
