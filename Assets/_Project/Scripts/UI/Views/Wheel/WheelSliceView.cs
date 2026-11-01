using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// One hole of the wheel: the reward icon and its amount. The two are placed with anchors rather than a layout
    /// group. The only change is the bomb, which has no amount, so its icon also takes the amount's space.
    /// </summary>
    public sealed class WheelSliceView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_wheel_slice_content";
        private const string IconName = "ui_image_wheel_slice_icon_value";
        private const string AmountName = "ui_text_wheel_slice_amount_value";

        [Tooltip("Distance from the slot's bottom edge to the icon's, with an amount under the icon and with the icon alone.")]
        [SerializeField] private float _iconBottomWithAmount = 37f;
        [SerializeField] private float _iconBottomAlone = 16f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        public RectTransform IconRect => _icon.rectTransform;

        public void Show(Sprite icon, string amount)
        {
            bool hasAmount = !string.IsNullOrEmpty(amount);
            _icon.sprite = icon;
            _amount.text = amount;
            _amount.enabled = hasAmount;

            RectTransform iconRect = _icon.rectTransform;
            iconRect.offsetMin = new Vector2(iconRect.offsetMin.x, hasAmount ? _iconBottomWithAmount : _iconBottomAlone);
        }

        public Tween Punch(float strength, float duration)
        {
            _content.DOKill(true);
            return _content.DOPunchScale(Vector3.one * strength, duration, 6, 0.6f);
        }

        private void OnDisable() => _content.DOKill(true);

        private void OnValidate()
        {
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _icon, IconName);
            transform.AssignIfFound(ref _amount, AmountName);
        }
    }
}
