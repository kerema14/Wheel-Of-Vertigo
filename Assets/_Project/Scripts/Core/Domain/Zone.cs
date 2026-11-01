using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }

    public sealed class Zone
    {
        public int Index { get; }
        public ZoneType Type { get; }
        public Wheel Wheel { get; }

        /// <summary>The reward the player is guaranteed on reaching this zone, or null. Only super zones have one.</summary>
        public RewardDefinition Unlock { get; }

        public bool HasUnlock => Unlock != null;

        public Zone(int index, ZoneType type, Wheel wheel, RewardDefinition unlock = null)
        {
            Index = index;
            Type = type;
            Wheel = wheel;
            Unlock = unlock;
        }
    }
}
