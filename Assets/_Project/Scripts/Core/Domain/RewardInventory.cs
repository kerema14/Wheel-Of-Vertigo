using System;
using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    public readonly struct RewardStack
    {
        public RewardDefinition Reward { get; }
        public int Amount { get; }

        public RewardStack(RewardDefinition reward, int amount)
        {
            Reward = reward;
            Amount = amount;
        }
    }

    /// <summary>
    /// The rewards collected during the current run. They are only kept if the player
    /// leaves at a safe or super zone; a bomb clears them.
    /// </summary>
    public sealed class RewardInventory
    {
        private readonly Dictionary<RewardDefinition, int> _amounts = new Dictionary<RewardDefinition, int>();
        private readonly List<RewardDefinition> _order = new List<RewardDefinition>();   // first collected first

        public int Count => _order.Count;

        public int Add(RewardDefinition reward, int amount)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (!_amounts.TryGetValue(reward, out int current))
                _order.Add(reward);

            int total = current + amount;
            _amounts[reward] = total;
            return total;
        }

        public int GetAmount(RewardDefinition reward) =>
            reward != null && _amounts.TryGetValue(reward, out int amount) ? amount : 0;

        public void Clear()
        {
            _amounts.Clear();
            _order.Clear();
        }

        /// <summary>A copy of the stacks, so callers can't change the inventory.</summary>
        public IReadOnlyList<RewardStack> Snapshot()
        {
            var stacks = new List<RewardStack>(_order.Count);
            foreach (RewardDefinition reward in _order)
                stacks.Add(new RewardStack(reward, _amounts[reward]));
            return stacks;
        }
    }
}
