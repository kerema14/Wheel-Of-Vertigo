using System;
using UnityEngine;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Core.Data
{
    /// <summary>
    /// The single tuning asset for a run: the zone types, reward growth, the bomb icon and the random seed.
    /// </summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Game Config", fileName = "game_config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Zones")]
        [Tooltip("The type of every zone that none of the zone types below claims.")]
        [SerializeField] private ZoneDefinition _defaultZone;
        [Tooltip("A zone takes the first type in this list whose interval divides its number, so put the rarer types first.")]
        [SerializeField] private ZoneDefinition[] _zones = Array.Empty<ZoneDefinition>();
        [Tooltip("0.15 means slice amounts grow by 15% of their base amount every zone.")]
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.15f;

        [Header("Bomb")]
        [SerializeField] private Sprite _bombIcon;

        [Header("Randomness")]
        [Tooltip("0 picks a new seed every run. Any other value replays the same spins, which helps testing.")]
        [SerializeField] private int _seed;

        public Sprite BombIcon => _bombIcon;
        public int Seed => _seed;

        public ZoneRules CreateRules() => new ZoneRules(_zones, _defaultZone, _rewardGrowthPerZone);

        private void OnValidate()
        {
            if (_defaultZone == null)
            {
                Debug.LogWarning($"{name}: no default zone.", this);
            }
            else
            {
                if (_defaultZone.Wheel != null && _defaultZone.Wheel.BombCount == 0)
                    Debug.LogWarning($"{name}: the default zone's wheel has no bomb, so the game has no risk.", this);
                if (Array.IndexOf(_zones, _defaultZone) >= 0)
                    Debug.LogWarning($"{name}: the default zone is also in the zone list.", this);
            }

            for (int i = 0; i < _zones.Length; i++)
            {
                ZoneDefinition zone = _zones[i];
                if (zone == null)
                {
                    Debug.LogWarning($"{name}: zone type {i} is empty.", this);
                    continue;
                }

                // The zones that interrupt the bomb wheel are the rewards for getting that far.
                if (zone.Wheel != null && zone.Wheel.BombCount > 0)
                    Debug.LogWarning($"{name}: {zone.name}'s wheel has a bomb; only the default zone is meant to.", this);

                for (int j = 0; j < i; j++)
                    if (_zones[j] != null && zone.Interval % _zones[j].Interval == 0)
                        Debug.LogWarning($"{name}: every {zone.name} zone is also a {_zones[j].name} zone, which comes first, so {zone.name} never appears. Move it up or change its interval.", this);
            }
        }
    }
}
