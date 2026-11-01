using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    public sealed class Zone
    {
        public int Index { get; }
        public ZoneDefinition Definition { get; }
        public Wheel Wheel { get; }

        /// <summary>The reward the player is guaranteed on reaching this zone, or null.</summary>
        public RewardDefinition Unlock { get; }

        public bool HasUnlock => Unlock != null;

        public Zone(int index, ZoneDefinition definition, Wheel wheel, RewardDefinition unlock = null)
        {
            Index = index;
            Definition = definition;
            Wheel = wheel;
            Unlock = unlock;
        }
    }
}
