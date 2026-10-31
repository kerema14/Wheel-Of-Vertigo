public class Zone
{
    public int Index { get; }
    
    public ZoneType ZoneType {  get; }

    public Wheel ZoneWheel { get; }

    public Zone(int zoneIndex, ZoneType zoneType, Wheel zoneWheel)
    {
        Index = zoneIndex;
        ZoneType = zoneType;
        ZoneWheel = zoneWheel;
    }
}
public enum ZoneType
{
    Normal,
    Safe,
    Super
}