using UnityEngine;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// The two cards that tell the player where the next safe and super zones are,
    /// and what the next super zone unlocks.
    /// </summary>
    public sealed class ZoneInfoView : MonoBehaviour
    {
        private const string SafeCardName = "ui_item_zone_info_card_safe";
        private const string SuperCardName = "ui_item_zone_info_card_super";

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private ZoneInfoCardView _safeCard;
        [SerializeField] private ZoneInfoCardView _superCard;

        public void Show(int nextSafeZone, int nextSuperZone, Sprite superZoneReward, ZoneType currentType)
        {
            _safeCard.Show(nextSafeZone);
            _superCard.Show(nextSuperZone, superZoneReward);

            // The card for the type of zone the player is standing in pulses.
            _safeCard.SetHighlighted(currentType == ZoneType.Safe);
            _superCard.SetHighlighted(currentType == ZoneType.Super);
        }

        private void OnValidate()
        {
            transform.AssignIfFound(ref _safeCard, SafeCardName);
            transform.AssignIfFound(ref _superCard, SuperCardName);
        }
    }
}
