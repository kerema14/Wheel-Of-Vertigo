using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>One hole of the wheel: the reward icon and its amount.</summary>
    public sealed class WheelSliceView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_wheel_slice_content";
        private const string IconName = "ui_image_wheel_slice_icon_value";
        private const string AmountName = "ui_text_wheel_slice_amount_value";

        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        public RectTransform IconRect => _icon.rectTransform;

        public void Show(Sprite icon, string amount)
        {
            _icon.sprite = icon;
            _amount.text = amount;
            _amount.enabled = !string.IsNullOrEmpty(amount);
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
