using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>A slice of the wheel for one zone, with its amount already scaled for that zone.</summary>
    public sealed class WheelSlice
    {
        public SliceType Type { get; }
        public RewardDefinition Reward { get; }   // null for the bomb
        public int Amount { get; }
        public bool IsBomb => Type == SliceType.Bomb;

        public WheelSlice(SliceType type, RewardDefinition reward, int amount)
        {
            Type = type;
            Reward = reward;
            Amount = amount;
        }
    }
}
