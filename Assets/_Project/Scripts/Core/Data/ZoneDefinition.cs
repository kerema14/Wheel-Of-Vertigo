using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfVertigo.Core.Data
{
    /// <summary>How a zone type looks on the zone bar.</summary>
    [Serializable]
    public struct ZoneStyle
    {
        [Tooltip("The zone's number on the bar while it is ahead or behind.")]
        public Color NumberColor;
        [Tooltip("The zone's number while it is the current zone, drawn on the panel below.")]
        public Color CurrentNumberColor;
        [Tooltip("The panel under the current zone's number.")]
        public Sprite CurrentPanel;
        public Color CurrentPanelColor;
    }

    /// <summary>
    /// One type of zone, such as safe or super: how often it comes, which wheel it uses, what reaching it
    /// unlocks and how it looks. A new zone type is a new asset in GameConfig's list, with no code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Zone Definition", fileName = "zone_")]
    public class ZoneDefinition : ScriptableObject
    {
        [Tooltip("Every Nth zone is this type. Not used by GameConfig's default zone, which covers every zone no other type claims.")]
        [SerializeField, Min(2)] private int _interval = 5;

        [SerializeField] private WheelConfig _wheel;

        [Tooltip("Reaching a zone of this type unlocks one of these, in order: the first such zone gets the first entry, and so on. The list repeats once it runs out. Leave it empty for no unlock.")]
        [SerializeField] private RewardDefinition[] _unlocks = Array.Empty<RewardDefinition>();

        [SerializeField] private ZoneStyle _style = new ZoneStyle
        {
            NumberColor = Color.white,
            CurrentNumberColor = Color.black,
            CurrentPanelColor = Color.white
        };

        public int Interval => _interval;
        public WheelConfig Wheel => _wheel;
        public IReadOnlyList<RewardDefinition> Unlocks => _unlocks;
        public ZoneStyle Style => _style;

        private void OnValidate()
        {
            if (_wheel == null)
                Debug.LogWarning($"{name}: no wheel assigned.", this);
            if (Array.IndexOf(_unlocks, null) >= 0)
                Debug.LogWarning($"{name}: an unlock entry is empty.", this);
        }
    }
}
