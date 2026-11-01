using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>Shown when the player leaves the run: a summary of what they walked away with.</summary>
    public sealed class CashOutPopupView : PopupView
    {
        private const string CollectButtonName = "ui_button_popup_cash_out_collect";
        private const string ListName = "ui_rect_popup_cash_out_list";

        [SerializeField] private RewardEntryView _entryPrefab;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _collectButton;
        [SerializeField] private RectTransform _list;

        private readonly List<RewardEntryView> _entries = new List<RewardEntryView>();

        public event Action CollectRequested;

        protected override string PopupId => "cash_out";

        public void Show(IReadOnlyList<RewardStack> rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                if (i == _entries.Count)
                    _entries.Add(Instantiate(_entryPrefab, _list));

                _entries[i].gameObject.SetActive(true);
                _entries[i].Bind(rewards[i].Reward.Icon, rewards[i].Amount);
            }

            for (int i = rewards.Count; i < _entries.Count; i++)
                _entries[i].gameObject.SetActive(false);

            Show();
        }

        protected override void OnShow()
        {
            foreach (RewardEntryView entry in _entries)
                if (entry.gameObject.activeSelf)
                    entry.PlayAppear();
        }

        private void OnEnable() => _collectButton.onClick.AddListener(HandleCollectClicked);

        protected override void OnDisable()
        {
            base.OnDisable();
            _collectButton.onClick.RemoveListener(HandleCollectClicked);
        }

        private void HandleCollectClicked() => CollectRequested?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            transform.AssignIfFound(ref _collectButton, CollectButtonName);
            transform.AssignIfFound(ref _list, ListName);
        }
    }
}
