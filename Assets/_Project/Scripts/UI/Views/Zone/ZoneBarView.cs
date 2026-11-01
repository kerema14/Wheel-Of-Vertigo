using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>
    /// The numbered strip at the top. The current zone always sits under the centre frame; moving to the next
    /// zone slides the strip one cell left and then rebinds the cells, so a fixed set of cells covers endless zones.
    /// Two strips slide together: the back strip of plain numbers, and a fore strip masked to the frame that holds
    /// the current and next zones' panels and numbers in their current-zone colours, so the next zone wipes in.
    /// Each zone's colours and panel come from its zone type's <see cref="ZoneStyle"/>.
    /// </summary>
    public sealed class ZoneBarView : MonoBehaviour
    {
        private const string ContentName = "ui_rect_zone_bar_content";
        private const string CurrentContentName = "ui_rect_zone_bar_current_content";
        private const string CurrentPanelName = "ui_image_zone_bar_current_value";
        private const string NextPanelName = "ui_image_zone_bar_next_value";

        [SerializeField, Min(1f)] private float _cellWidth = 90f;
        [SerializeField, Min(0.05f)] private float _slideDuration = 0.45f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;

        [SerializeField, Range(0f, 1f)] private float _passedZoneAlpha = 0.35f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform _currentContent;
        [SerializeField] private Image _currentPanel;
        [SerializeField] private Image _nextPanel;
        [SerializeField] private ZoneBarItemView[] _items = Array.Empty<ZoneBarItemView>();
        [SerializeField] private ZoneBarItemView[] _currentItems = Array.Empty<ZoneBarItemView>();

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

            _slideTween = DOVirtual.Float(0f, 1f, _slideDuration, SetSlideProgress)
                .SetEase(_slideEase)
                .OnComplete(() => SetZone(zone));
        }

        /// <summary>
        /// Numbers move one cell and panels one panel width, so the next panel fills the frame exactly as its
        /// number arrives under it. The seam between the panels stays between the two numbers the whole way.
        /// </summary>
        private void SetSlideProgress(float progress)
        {
            float panelWidth = _currentPanel.rectTransform.rect.width;
            SetX(_content, -_cellWidth * progress);
            SetX(_currentContent, -_cellWidth * progress);
            SetX(_currentPanel.rectTransform, -panelWidth * progress);
            SetX(_nextPanel.rectTransform, panelWidth * (1f - progress));
        }

        private void SetZone(int zone)
        {
            _zone = zone;
            SetSlideProgress(0f);

            for (int i = 0; i < _items.Length; i++)
            {
                int cellZone = zone + i - CentreIndex;
                _items[i].Show(cellZone, GetNumberColor(cellZone));
            }

            for (int i = 0; i < _currentItems.Length; i++)
            {
                int cellZone = zone + i;
                _currentItems[i].Show(cellZone, GetStyle(cellZone).CurrentNumberColor);
            }

            ShowPanel(_currentPanel, GetStyle(zone));
            ShowPanel(_nextPanel, GetStyle(zone + 1));
        }

        /// <summary>
        /// The back strip's colour. The current zone's cell is covered by the fore strip, so it is already drawn
        /// as passed and needs no change when it slides out from under the frame.
        /// </summary>
        private Color GetNumberColor(int cellZone)
        {
            Color color = GetStyle(cellZone).NumberColor;
            if (cellZone <= _zone) color.a *= _passedZoneAlpha;
            return color;
        }

        private static void ShowPanel(Image panel, ZoneStyle style)
        {
            panel.sprite = style.CurrentPanel;
            panel.color = style.CurrentPanelColor;
        }

        private static void SetX(RectTransform rect, float x) => rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);

        private ZoneStyle GetStyle(int zone) => _rules.GetZone(zone).Style;

        /// <summary>Lines the back cells up left to right with the middle one at x = 0, and the fore cells from x = 0.</summary>
        [ContextMenu("Layout Cells")]
        private void LayoutCells()
        {
            for (int i = 0; i < _items.Length; i++)
                ((RectTransform)_items[i].transform).anchoredPosition = new Vector2((i - CentreIndex) * _cellWidth, 0f);
            for (int i = 0; i < _currentItems.Length; i++)
                ((RectTransform)_currentItems[i].transform).anchoredPosition = new Vector2(i * _cellWidth, 0f);
        }

        private void Awake() => LayoutCells();

        private void OnDisable() => _slideTween?.Kill();

        private void OnValidate()
        {
            transform.AssignIfFound(ref _content, ContentName);
            transform.AssignIfFound(ref _currentContent, CurrentContentName);
            transform.AssignIfFound(ref _currentPanel, CurrentPanelName);
            transform.AssignIfFound(ref _nextPanel, NextPanelName);

            if (_content != null)
                _items = _content.GetComponentsInChildren<ZoneBarItemView>(true);
            if (_currentContent != null)
                _currentItems = _currentContent.GetComponentsInChildren<ZoneBarItemView>(true);
        }
    }
}
