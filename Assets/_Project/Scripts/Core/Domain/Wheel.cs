using System;
using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>The wheel the player spins in one zone.</summary>
    public sealed class Wheel
    {
        public WheelConfig Config { get; }   // the view still needs the sprites and title; null in tests
        public IReadOnlyList<WheelSlice> Slices { get; }
        public int SliceCount => Slices.Count;

        public Wheel(WheelConfig config, IReadOnlyList<WheelSlice> slices)
        {
            if (slices == null || slices.Count == 0)
                throw new ArgumentException("A wheel needs at least one slice.", nameof(slices));

            Config = config;
            Slices = slices;
        }

        public WheelSlice GetSlice(int index) => Slices[index];
    }
}
