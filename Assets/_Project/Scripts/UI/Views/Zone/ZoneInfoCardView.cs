using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// One zone info card: the next zone of its zone type and, when that zone has one, the reward waiting there.
    /// Each zone type's card is a variant of the card prefab that names its <see cref="ZoneDefinition"/>.
    /// </summary>
    public sealed class ZoneInfoCardView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_zone_info_card_content";
        private const string ZoneTextName = "ui_text_zone_info_card_zone_value";
        private const string RewardName = "ui_rect_zone_info_card_reward";
        private const string RewardIconName = "ui_image_zone_info_card_reward_value";

        [Tooltip("The zone type this card is about.")]
        [SerializeField] private ZoneDefinition _zone;
        [SerializeField, Min(1f)] private float _pulseScale = 1.08f;
        [SerializeField, Min(0.05f)] private float _pulseDuration = 0.5f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text _zoneText;
        [SerializeField] private RectTransform _reward;
        [SerializeField] private Image _rewardIcon;

        public ZoneDefinition Zone => _zone;

        public void Show(int zone, Sprite reward = null)
        {
            _zoneText.text = zone.ToString();
            _rewardIcon.sprite = reward;
            _reward.gameObject.SetActive(reward != null);   // without a reward, the number takes its place
        }

        /// <summary>Pulses the card while the player stands in a zone of its type. The content pulses, not the root.</summary>
        public void SetHighlighted(bool highlighted)
        {
            _content.DOKill();
            _content.localScale = Vector3.one;

            if (highlighted)
                _content.DOScale(_pulseScale, _pulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable() => _content.DOKill();

        private void OnValidate()
        {
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _zoneText, ZoneTextName);
            transform.AssignIfFound(ref _reward, RewardName);
            transform.AssignIfFound(ref _rewardIcon, RewardIconName);
        }
    }
}
