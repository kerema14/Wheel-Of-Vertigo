using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// Shared show/hide behaviour for full-screen popups. Each popup is its own Canvas, so its animations never
    /// rebuild the rest of the UI. A hidden popup only has its Canvas and GraphicRaycaster turned off: nothing is
    /// drawn or hit, and its meshes are kept for the next time it shows. The root never moves; the dim and the
    /// content child do the animating.
    /// The popup canvases sort above the HUD (sorting order 1). The root canvas has no raycaster, so Unity doesn't
    /// compare depth across the per-panel raycasters; the sorting order is what puts the dim's hit first.
    /// </summary>
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _dimAlpha = 0.85f;
        [SerializeField, Min(0.05f)] private float _showDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float _hideDuration = 0.2f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GraphicRaycaster _raycaster;
        [SerializeField] private Image _dim;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CanvasGroup _contentGroup;

        private Sequence _sequence;

        /// <summary>The middle part of the child names, e.g. "bomb" for ui_image_popup_bomb_dim.</summary>
        protected abstract string PopupId { get; }

        public void Show()
        {
            _sequence?.Kill();
            SetShown(true);

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
                    SetShown(false);
                    OnHidden();
                    onHidden?.Invoke();
                });
        }

        /// <summary>Lets a popup add its own flourish once it starts showing.</summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>Lets a popup stop anything it left running, such as a looping tween.</summary>
        protected virtual void OnHidden()
        {
        }

        // Popups start hidden whatever the scene says, so turning one on to edit it can't leave it showing.
        protected virtual void Awake() => SetShown(false);

        protected virtual void OnDisable() => _sequence?.Kill();

        protected virtual void OnValidate()
        {
            TryGetComponent(out _canvas);
            TryGetComponent(out _raycaster);
            transform.AssignIfFound(ref _dim, $"ui_image_popup_{PopupId}_dim");
            transform.AssignIfFound(ref _content, $"ui_rect_popup_{PopupId}_content");
            transform.AssignIfFound(ref _contentGroup, $"ui_rect_popup_{PopupId}_content");
        }

        private void SetShown(bool shown)
        {
            _canvas.enabled = shown;
            _raycaster.enabled = shown;
        }

        private void SetDimAlpha(float alpha)
        {
            Color color = _dim.color;
            color.a = alpha;
            _dim.color = color;
        }
    }
}
