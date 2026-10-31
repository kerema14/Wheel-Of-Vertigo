using System;

namespace WheelOfVertigo.Core.Domain
{
    /// <summary>Picks the slice a spin lands on. Swapped for a fixed picker in tests.</summary>
    public interface ISlicePicker
    {
        int Pick(Wheel wheel);
    }

    /// <summary>Every slice is equally likely. A seed makes a run repeatable.</summary>
    public sealed class RandomSlicePicker : ISlicePicker
    {
        private readonly Random _random;

        public RandomSlicePicker(int seed)
        {
            _random = seed == 0 ? new Random() : new Random(seed);
        }

        public int Pick(Wheel wheel) => _random.Next(wheel.SliceCount);
    }
}
