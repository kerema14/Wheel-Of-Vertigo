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

        public Zone(int index, ZoneType type, Wheel wheel)
        {
            Index = index;
            Type = type;
            Wheel = wheel;
        }
    }
}
