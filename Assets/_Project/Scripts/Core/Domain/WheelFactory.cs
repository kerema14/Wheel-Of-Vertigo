using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>Builds the wheel for a zone. Tests supply their own wheels without any assets.</summary>
    public interface IWheelFactory
    {
        Wheel Create(int zone, ZoneType zoneType);
    }

    /// <summary>Builds wheels from the wheel config that GameConfig assigns to each zone type.</summary>
    public sealed class WheelFactory : IWheelFactory
    {
        private readonly GameConfig _config;
        private readonly ZoneRules _rules;

        public WheelFactory(GameConfig config, ZoneRules rules)
        {
            _config = config;
            _rules = rules;
        }

        public Wheel Create(int zone, ZoneType zoneType)
        {
            WheelConfig wheelConfig = _config.GetWheelConfig(zoneType);
            var slices = new List<WheelSlice>(wheelConfig.Slices.Count);

            foreach (SliceData data in wheelConfig.Slices)
            {
                int amount = data.Type == SliceType.Bomb ? 0 : _rules.ScaleAmount(data.BaseAmount, zone);
                slices.Add(new WheelSlice(data.Type, data.Reward, amount));
            }

            return new Wheel(wheelConfig, slices);
        }
    }
}
