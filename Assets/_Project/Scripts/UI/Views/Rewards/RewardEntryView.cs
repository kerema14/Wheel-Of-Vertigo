using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Utils;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>An icon with an amount. Used by the rewards panel and the cash-out popup.</summary>
    public sealed class RewardEntryView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_reward_entry_content";
        private const string IconName = "ui_image_reward_entry_icon_value";
        private const string AmountName = "ui_text_reward_entry_amount_value";

        [SerializeField, Min(0.05f)] private float _countDuration = 0.5f;
        [SerializeField, Min(0.05f)] private float _appearDuration = 0.25f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        private int _shownAmount;
        private Tween _countTween;

        public RectTransform IconRect => _icon.rectTransform;

        public void Bind(Sprite icon, int amount)
        {
            _countTween?.Kill();
            _icon.sprite = icon;
            SetShownAmount(amount);
        }

        public void AnimateAmount(int amount)
        {
            _countTween?.Kill();
            _countTween = DOTween.To(() => _shownAmount, SetShownAmount, amount, _countDuration).SetEase(Ease.OutQuad);
        }

        public void PlayAppear()
        {
            _content.DOKill(true);
            _content.localScale = Vector3.zero;
            _content.DOScale(1f, _appearDuration).SetEase(Ease.OutBack);
        }

        public void Punch()
        {
            _content.DOKill(true);
            _content.DOPunchScale(Vector3.one * 0.2f, 0.2f, 4, 0.5f);
        }

        private void SetShownAmount(int amount)
        {
            _shownAmount = amount;
            _amount.text = AmountFormatter.Format(amount);
        }

        private void OnDisable()
        {
            _countTween?.Kill();
            _content.DOKill(true);
        }

        private void OnValidate()
        {
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _icon, IconName);
            transform.AssignIfFound(ref _amount, AmountName);
        }
    }
}
