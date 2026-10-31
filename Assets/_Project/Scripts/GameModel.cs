
public class GameModel
{
    public WheelSlice CurrentSlice { get; internal set; }

    public Zone CurrentZone { get; internal set; }
    public Wheel CurrentWheel => CurrentZone.ZoneWheel;

    public GameModel(Zone currentZone)
    {        
        CurrentZone = currentZone;
    }

}
