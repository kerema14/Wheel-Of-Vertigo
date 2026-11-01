using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>Builds the wheel for a zone. Tests supply their own wheels without any assets.</summary>
    public interface IWheelFactory
    {
        Wheel Create(int zone, ZoneDefinition definition);
    }

    /// <summary>Builds a zone's wheel from its zone type's wheel config, with amounts grown for the zone.</summary>
    public sealed class WheelFactory : IWheelFactory
    {
        private readonly ZoneRules _rules;

        public WheelFactory(ZoneRules rules) => _rules = rules;

        public Wheel Create(int zone, ZoneDefinition definition)
        {
            WheelConfig wheelConfig = definition.Wheel;
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
