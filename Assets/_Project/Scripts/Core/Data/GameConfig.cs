using UnityEngine;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Core.Data
{
    /// <summary>
    /// The single tuning asset for a run: zone rhythm, reward growth and which wheel each zone type uses.
    /// </summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Game Config", fileName = "game_config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Zones")]
        [SerializeField, Min(2)] private int _safeZoneInterval = 5;
        [SerializeField, Min(2)] private int _superZoneInterval = 30;
        [Tooltip("0.15 means slice amounts grow by 15% of their base amount every zone.")]
        [SerializeField, Min(0f)] private float _rewardGrowthPerZone = 0.15f;

        [Header("Wheels")]
        [SerializeField] private WheelConfig _normalWheel;
        [SerializeField] private WheelConfig _safeWheel;
        [SerializeField] private WheelConfig _superWheel;

        [Header("Bomb")]
        [SerializeField] private Sprite _bombIcon;

        [Header("Randomness")]
        [Tooltip("0 picks a new seed every run. Any other value replays the same spins, which helps testing.")]
        [SerializeField] private int _seed;

        public Sprite BombIcon => _bombIcon;
        public int Seed => _seed;

        public ZoneRules CreateRules() => new ZoneRules(_safeZoneInterval, _superZoneInterval, _rewardGrowthPerZone);

        public WheelConfig GetWheelConfig(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe: return _safeWheel;
                case ZoneType.Super: return _superWheel;
                default: return _normalWheel;
            }
        }

        private void OnValidate()
        {
            if (_superZoneInterval % _safeZoneInterval != 0)
                Debug.LogWarning($"{name}: the super zone interval should be a multiple of the safe zone interval.", this);

            if (_normalWheel != null && _normalWheel.BombCount == 0)
                Debug.LogWarning($"{name}: the normal wheel has no bomb, so the game has no risk.", this);

            WarnIfHasBomb(_safeWheel, "safe");
            WarnIfHasBomb(_superWheel, "super");
        }

        private void WarnIfHasBomb(WheelConfig wheel, string zoneName)
        {
            if (wheel != null && wheel.BombCount > 0)
                Debug.LogWarning($"{name}: the {zoneName} wheel must not contain a bomb.", this);
        }
    }
}
