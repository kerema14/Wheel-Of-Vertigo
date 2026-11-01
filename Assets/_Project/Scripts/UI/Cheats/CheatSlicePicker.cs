#if UNITY_EDITOR || DEVELOPMENT_BUILD
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.UI.Cheats
{
    /// <summary>Picks like the picker it wraps, unless the cheat menu has chosen where the next spin lands.</summary>
    internal sealed class CheatSlicePicker : ISlicePicker
    {
        private readonly ISlicePicker _picker;

        /// <summary>The slice the next spin lands on, or null to pick normally. Used up by that spin.</summary>
        public int? Forced { get; set; }

        public CheatSlicePicker(ISlicePicker picker) => _picker = picker;

        public int Pick(Wheel wheel)
        {
            if (Forced is int index && index < wheel.SliceCount)
            {
                Forced = null;
                return index;
            }

            Forced = null;
            return _picker.Pick(wheel);
        }
    }
}
#endif
