using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.Core.Utils;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// Shows a wheel and plays its animations. It never decides an outcome:
    /// the presenter tells it which slice to land on.
    /// </summary>
    public sealed class WheelView : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_wheel_spin";
        private const string ContentName = "ui_rect_wheel_content";
        private const string SpinRootName = "ui_rect_wheel_spin_root";
        private const string BaseImageName = "ui_image_wheel_base_value";
        private const string IndicatorImageName = "ui_image_wheel_indicator_value";
        private const string TitleTextName = "ui_text_wheel_title_value";
        private const string SubtitleTextName = "ui_text_wheel_subtitle_value";

        [SerializeField] private WheelSpinSettings _settings;
        [SerializeField, Min(0f)] private float _sliceRadius = 143f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _spinButton;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _spinRoot;
        [SerializeField] private Image _baseImage;
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private WheelSliceView[] _slices = Array.Empty<WheelSliceView>();

        private float _angle;   // unwrapped, so a spin always knows how far it has turned
        private int _lastTickSlice;
        private Tween _spinTween;
        private Tween _indicatorTween;
        private Sequence _contentSequence;

        public event Action SpinRequested;

        private float SliceAngle => 360f / _slices.Length;

        public void SetSpinInteractable(bool interactable) => _spinButton.interactable = interactable;

        public RectTransform GetSliceIcon(int sliceIndex) => _slices[sliceIndex].IconRect;

        public void Bind(Wheel wheel, Sprite bombIcon)
        {
            WheelConfig config = wheel.Config;
            _baseImage.sprite = config.BaseSprite;
            _indicatorImage.sprite = config.IndicatorSprite;
            _titleText.text = config.Title;
            _titleText.color = config.TitleColor;
            _subtitleText.text = config.Subtitle;

            if (wheel.SliceCount != _slices.Length)
                Debug.LogError($"{name}: the wheel has {wheel.SliceCount} slices but the view has {_slices.Length} slots.", this);

            int count = Mathf.Min(wheel.SliceCount, _slices.Length);
            for (int i = 0; i < count; i++)
            {
                WheelSlice slice = wheel.GetSlice(i);
                if (slice.IsBomb)
                    _slices[i].Show(bombIcon, string.Empty);
                else
                    _slices[i].Show(slice.Reward.Icon, AmountFormatter.FormatMultiplier(slice.Amount));
            }
        }

        /// <summary>Spins clockwise from wherever the wheel is now and stops with the given slice under the indicator.</summary>
        public void PlaySpin(int sliceIndex, Action onComplete)
        {
            _spinTween?.Kill();

            float jitter = UnityEngine.Random.Range(-1f, 1f) * _settings.LandingJitter * SliceAngle * 0.5f;
            float landing = sliceIndex * SliceAngle + jitter;
            float clockwiseDistance = Mathf.Repeat(_angle - landing, 360f);
            float target = _angle - clockwiseDistance - 360f * _settings.FullTurns;

            _lastTickSlice = TickSliceAt(_angle);
            _spinTween = DOTween.To(() => _angle, SetAngle, target, _settings.SpinDuration)
                .SetEase(_settings.SpinEase)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void PlayWin(int sliceIndex) =>
            _slices[sliceIndex].Punch(_settings.WinPunchScale, _settings.WinPunchDuration);

        public void PlayBomb(int sliceIndex, Action onComplete)
        {
            _slices[sliceIndex].Punch(_settings.WinPunchScale, _settings.WinPunchDuration);

            ResetContentSequence();
            _contentSequence.Append(_content.DOShakeAnchorPos(_settings.BombShakeDuration, _settings.BombShakeStrength, 30))
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>Shrinks the wheel away, lets the caller swap it while hidden, then pops it back.</summary>
        public void PlayTierChange(Action onHidden, Action onComplete)
        {
            float half = _settings.TierChangeDuration;
            ResetContentSequence();
            _contentSequence.Append(_content.DOScale(0f, half).SetEase(Ease.InBack))
                .AppendCallback(() => onHidden?.Invoke())
                .Append(_content.DOScale(1f, half * 1.4f).SetEase(Ease.OutBack))
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>A light pulse across the slices, so the player notices the amounts grew.</summary>
        public void PlayRefresh()
        {
            for (int i = 0; i < _slices.Length; i++)
                _slices[i].Punch(_settings.WinPunchScale * 0.4f, _settings.WinPunchDuration * 0.6f).SetDelay(i * 0.03f);
        }

        private void SetAngle(float angle)
        {
            _angle = angle;
            _spinRoot.localRotation = Quaternion.Euler(0f, 0f, angle);

            int tickSlice = TickSliceAt(angle);
            if (tickSlice == _lastTickSlice) return;

            _lastTickSlice = tickSlice;
            TickIndicator();
        }

        // Counts slice borders passing the indicator, so it flicks once per border like a real peg.
        private int TickSliceAt(float angle) => Mathf.FloorToInt((angle + SliceAngle * 0.5f) / SliceAngle);

        private void TickIndicator()
        {
            RectTransform indicator = _indicatorImage.rectTransform;
            _indicatorTween?.Kill();
            indicator.localRotation = Quaternion.Euler(0f, 0f, _settings.IndicatorTickAngle);
            _indicatorTween = indicator.DOLocalRotate(Vector3.zero, _settings.IndicatorTickDuration).SetEase(Ease.OutQuad);
        }

        private void ResetContentSequence()
        {
            _contentSequence?.Kill(true);
            _contentSequence = DOTween.Sequence();
        }

        /// <summary>Places the slice slots evenly around the wheel, clockwise from the top. Slot i sits at i * 360 / count degrees.</summary>
        [ContextMenu("Layout Slices")]
        private void LayoutSlices()
        {
            for (int i = 0; i < _slices.Length; i++)
            {
                float degrees = i * 360f / _slices.Length;
                float radians = degrees * Mathf.Deg2Rad;
                var slot = (RectTransform)_slices[i].transform;
                slot.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * _sliceRadius;
                slot.localRotation = Quaternion.Euler(0f, 0f, -degrees);
            }
        }

        private void Awake()
        {
            LayoutSlices();
            _angle = _spinRoot.localEulerAngles.z;
        }

        private void OnEnable() => _spinButton.onClick.AddListener(HandleSpinClicked);

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(HandleSpinClicked);
            _spinTween?.Kill();   // don't let a running spin call back into a disabled view
            _indicatorTween?.Kill();
            _contentSequence?.Kill();
        }

        private void HandleSpinClicked() => SpinRequested?.Invoke();

        private void OnValidate()
        {
            transform.AssignIfFound(ref _spinButton, SpinButtonName);
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _spinRoot, SpinRootName);
            transform.AssignIfFound(ref _baseImage, BaseImageName);
            transform.AssignIfFound(ref _indicatorImage, IndicatorImageName);
            transform.AssignIfFound(ref _titleText, TitleTextName);
            transform.AssignIfFound(ref _subtitleText, SubtitleTextName);

            if (_spinRoot != null)
                _slices = _spinRoot.GetComponentsInChildren<WheelSliceView>(true);
        }
    }
}
