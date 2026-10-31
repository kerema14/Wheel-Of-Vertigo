using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// Shared show/hide behaviour for full-screen popups. The root only toggles active; the dim and the
    /// content child do the animating, so no tween ever moves the popup's root transform.
    /// </summary>
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _dimAlpha = 0.85f;
        [SerializeField, Min(0.05f)] private float _showDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float _hideDuration = 0.2f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Image _dim;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CanvasGroup _contentGroup;

        private Sequence _sequence;

        /// <summary>The middle part of the child names, e.g. "bomb" for ui_image_popup_bomb_dim.</summary>
        protected abstract string PopupId { get; }

        public void Show()
        {
            gameObject.SetActive(true);
            _sequence?.Kill();

            SetDimAlpha(0f);
            _content.localScale = Vector3.one * 0.7f;
            _contentGroup.alpha = 0f;
            _contentGroup.interactable = false;

            _sequence = DOTween.Sequence()
                .Append(_dim.DOFade(_dimAlpha, _showDuration * 0.6f))
                .Join(_content.DOScale(1f, _showDuration).SetEase(Ease.OutBack))
                .Join(_contentGroup.DOFade(1f, _showDuration * 0.6f))
                .OnComplete(() => _contentGroup.interactable = true);

            OnShow();
        }

        public void Hide(Action onHidden = null)
        {
            _sequence?.Kill();
            _contentGroup.interactable = false;

            _sequence = DOTween.Sequence()
                .Append(_content.DOScale(0.8f, _hideDuration).SetEase(Ease.InBack))
                .Join(_contentGroup.DOFade(0f, _hideDuration))
                .Join(_dim.DOFade(0f, _hideDuration))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onHidden?.Invoke();
                });
        }

        /// <summary>Lets a popup add its own flourish once it starts showing.</summary>
        protected virtual void OnShow()
        {
        }

        protected virtual void OnDisable() => _sequence?.Kill();

        protected virtual void OnValidate()
        {
            transform.AssignIfFound(ref _dim, $"ui_image_popup_{PopupId}_dim");
            transform.AssignIfFound(ref _content, $"ui_rect_popup_{PopupId}_content");
            transform.AssignIfFound(ref _contentGroup, $"ui_rect_popup_{PopupId}_content");
        }

        private void SetDimAlpha(float alpha)
        {
            Color color = _dim.color;
            color.a = alpha;
            _dim.color = color;
        }
    }
}
