using UnityEngine;

namespace WheelOfVertigo.Core.Data
{
    public enum RewardKind
    {
        Currency,
        Chest,
        Points,
        Consumable,
        Cosmetic,
        Weapon
    }

    /// <summary>
    /// What a reward is: its name, icon and kind. How much of it a slice gives
    /// is decided by the wheel config, so one definition is shared by every wheel.
    /// </summary>
    [CreateAssetMenu(menuName = "Wheel Of Vertigo/Reward Definition", fileName = "reward_")]
    public class RewardDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private RewardKind _kind;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public RewardKind Kind => _kind;
    }
}
