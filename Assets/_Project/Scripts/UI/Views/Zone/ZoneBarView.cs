using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// The numbered strip at the top. The current zone always sits under the centre frame; moving to the next
    /// zone slides the strip one cell left and then rebinds the cells, so a fixed set of cells covers endless zones.
    /// </summary>
    public sealed class ZoneBarView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_zone_bar_content";
        private const string CurrentFrameName = "ui_image_zone_bar_current_value";

        [Serializable]
        private struct ZoneStyle
        {
            public Color NumberColor;
            public Color CurrentNumberColor;
            public Sprite CurrentFrame;
            public Color CurrentFrameColor;
        }

        [SerializeField, Min(1f)] private float _cellWidth = 90f;
        [SerializeField, Min(0.05f)] private float _slideDuration = 0.45f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;

        [Header("Styles")]
        [SerializeField] private ZoneStyle _normalStyle;
        [SerializeField] private ZoneStyle _safeStyle;
        [SerializeField] private ZoneStyle _superStyle;
        [SerializeField, Range(0f, 1f)] private float _passedZoneAlpha = 0.35f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _currentFrame;
        [SerializeField] private ZoneBarItemView[] _items = Array.Empty<ZoneBarItemView>();

        private ZoneRules _rules;
        private int _zone = ZoneRules.FirstZone;
        private Tween _slideTween;

        private int CentreIndex => _items.Length / 2;

        public void Initialize(ZoneRules rules) => _rules = rules;

        public void ShowZone(int zone, bool animate)
        {
            _slideTween?.Kill();

            if (!animate || zone != _zone + 1)
            {
                SetZone(zone);
                return;
            }

            _slideTween = _content.DOAnchorPosX(-_cellWidth, _slideDuration)
                .SetEase(_slideEase)
                .OnComplete(() => SetZone(zone));
        }

        private void SetZone(int zone)
        {
            _zone = zone;
            _content.anchoredPosition = new Vector2(0f, _content.anchoredPosition.y);

            for (int i = 0; i < _items.Length; i++)
            {
                int cellZone = zone + i - CentreIndex;
                _items[i].Show(cellZone, GetNumberColor(cellZone));
            }

            ZoneStyle style = GetStyle(zone);
            _currentFrame.sprite = style.CurrentFrame;
            _currentFrame.color = style.CurrentFrameColor;
        }

        private Color GetNumberColor(int cellZone)
        {
            if (cellZone == _zone) return GetStyle(cellZone).CurrentNumberColor;

            Color color = GetStyle(cellZone).NumberColor;
            if (cellZone < _zone) color.a *= _passedZoneAlpha;
            return color;
        }

        private ZoneStyle GetStyle(int zone)
        {
            switch (_rules.GetZoneType(zone))
            {
                case ZoneType.Safe: return _safeStyle;
                case ZoneType.Super: return _superStyle;
                default: return _normalStyle;
            }
        }

        /// <summary>Lines the cells up left to right with the middle one at x = 0.</summary>
        [ContextMenu("Layout Cells")]
        private void LayoutCells()
        {
            for (int i = 0; i < _items.Length; i++)
                ((RectTransform)_items[i].transform).anchoredPosition = new Vector2((i - CentreIndex) * _cellWidth, 0f);
        }

        private void Awake() => LayoutCells();

        private void OnDisable() => _slideTween?.Kill();

        private void OnValidate()
        {
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _currentFrame, CurrentFrameName);

            if (_content != null)
                _items = _content.GetComponentsInChildren<ZoneBarItemView>(true);
        }
    }
}
