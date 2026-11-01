using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        private const string SafeCardName = "ui_rect_zone_info_safe_content";
        private const string SuperCardName = "ui_rect_zone_info_super_content";
        private const string SafeZoneTextName = "ui_text_zone_info_safe_zone_value";
        private const string SuperZoneTextName = "ui_text_zone_info_super_zone_value";
        private const string SuperRewardName = "ui_rect_zone_info_super_reward";
        private const string SuperRewardIconName = "ui_image_zone_info_super_reward_value";

        [SerializeField, Min(1f)] private float _pulseScale = 1.08f;
        [SerializeField, Min(0.05f)] private float _pulseDuration = 0.5f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _safeCard;
        [SerializeField] private RectTransform _superCard;
        [SerializeField] private TMP_Text _safeZoneText;
        [SerializeField] private TMP_Text _superZoneText;
        [SerializeField] private RectTransform _superReward;
        [SerializeField] private Image _superRewardIcon;

        public void Show(int nextSafeZone, int nextSuperZone, Sprite superZoneReward, ZoneType currentType)
        {
            _safeZoneText.text = nextSafeZone.ToString();
            _superZoneText.text = nextSuperZone.ToString();
            _superRewardIcon.sprite = superZoneReward;
            _superReward.gameObject.SetActive(superZoneReward != null);   // without an unlock, the number takes its place

            // The card for the zone the player is standing in pulses, as a hint that leaving is allowed now.
            SetPulse(_safeCard, currentType == ZoneType.Safe);
            SetPulse(_superCard, currentType == ZoneType.Super);
        }

        private void SetPulse(RectTransform card, bool pulse)
        {
            card.DOKill();
            card.localScale = Vector3.one;

            if (pulse)
                card.DOScale(_pulseScale, _pulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            _safeCard.DOKill();
            _superCard.DOKill();
        }

        private void OnValidate()
        {
            transform.AssignIfFound(ref _safeCard, SafeCardName);
            transform.AssignIfFound(ref _superCard, SuperCardName);
            transform.AssignIfFound(ref _safeZoneText, SafeZoneTextName);
            transform.AssignIfFound(ref _superZoneText, SuperZoneTextName);
            transform.AssignIfFound(ref _superReward, SuperRewardName);
            transform.AssignIfFound(ref _superRewardIcon, SuperRewardIconName);
        }
    }
}
