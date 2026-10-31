using System;
using System.Collections.Generic;


public class Wheel
{
    private Dictionary<int, WheelSlice> _wheelSlices;
    public IReadOnlyList<WheelSlice> Slices;
    public WheelConfig Config { get; private set; }
    public Wheel(WheelConfig config, int zone, ZoneRules rules)
    {
        Config = config;   // the view still needs the sprites
        var slices = new List<WheelSlice>(config.Slices.Count);
        foreach (SliceData s in config.Slices)
            slices.Add(new WheelSlice(s.Type, s.Reward, rules.ScaleAmount(s.BaseAmount, zone)));
        Slices = slices;   // IReadOnlyList<WheelSlice>
    }
    public WheelSlice GetSlice(int index)
    {
        return Slices[index];
    }
}