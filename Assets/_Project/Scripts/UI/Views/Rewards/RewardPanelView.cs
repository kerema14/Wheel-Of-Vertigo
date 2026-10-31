using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>The left panel: the rewards collected this run and the exit button.</summary>
    public sealed class RewardPanelView : MonoBehaviour
    {
        private const string ExitButtonName = "ui_button_rewards_exit";
        private const string ScrollName = "ui_scroll_rewards";
        private const string ListName = "ui_rect_rewards_list";

        [SerializeField] private RewardEntryView _entryPrefab;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _exitButton;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private RectTransform _list;

        private readonly Dictionary<RewardDefinition, RewardEntryView> _entries = new Dictionary<RewardDefinition, RewardEntryView>();
        private readonly Stack<RewardEntryView> _pool = new Stack<RewardEntryView>();

        public event Action ExitRequested;

        public void SetExitInteractable(bool interactable) => _exitButton.interactable = interactable;

        /// <summary>
        /// Returns the entry for a reward, adding an empty one if it's new so flying icons have somewhere to land.
        /// The new entry is laid out and scrolled into view straight away.
        /// </summary>
        public RewardEntryView GetOrCreateEntry(RewardDefinition reward)
        {
            if (_entries.TryGetValue(reward, out RewardEntryView entry))
            {
                ScrollTo(entry);
                return entry;
            }

            entry = _pool.Count > 0 ? _pool.Pop() : Instantiate(_entryPrefab, _list);
            entry.gameObject.SetActive(true);
            entry.transform.SetAsLastSibling();
            entry.Bind(reward.Icon, 0);
            _entries.Add(reward, entry);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_list);
            ScrollTo(entry);
            entry.PlayAppear();
            return entry;
        }

        public void Clear()
        {
            foreach (RewardEntryView entry in _entries.Values)
            {
                entry.gameObject.SetActive(false);
                _pool.Push(entry);
            }

            _entries.Clear();
            _scroll.verticalNormalizedPosition = 1f;
        }

        // Moves the list just enough for the entry to be fully visible.
        private void ScrollTo(RewardEntryView entry)
        {
            RectTransform viewport = _scroll.viewport;
            float hiddenHeight = _list.rect.height - viewport.rect.height;
            if (hiddenHeight <= 0f) return;

            var entryRect = (RectTransform)entry.transform;
            float entryTop = -entryRect.anchoredPosition.y - entryRect.rect.height * (1f - entryRect.pivot.y);
            float entryBottom = entryTop + entryRect.rect.height;
            float scrollTop = (1f - _scroll.verticalNormalizedPosition) * hiddenHeight;

            if (entryBottom > scrollTop + viewport.rect.height)
                scrollTop = entryBottom - viewport.rect.height;
            else if (entryTop < scrollTop)
                scrollTop = entryTop;

            _scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(scrollTop / hiddenHeight);
        }

        private void OnEnable() => _exitButton.onClick.AddListener(HandleExitClicked);

        private void OnDisable() => _exitButton.onClick.RemoveListener(HandleExitClicked);

        private void HandleExitClicked() => ExitRequested?.Invoke();

        private void OnValidate()
        {
            transform.AssignIfFound(ref _exitButton, ExitButtonName);
            transform.AssignIfFound(ref _scroll, ScrollName);
            transform.AssignIfFound(ref _list, ListName);
        }
    }
}
