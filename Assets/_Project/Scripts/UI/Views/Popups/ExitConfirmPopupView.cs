using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>Asks the player to confirm before leaving the run, so a stray tap on Exit doesn't end it.</summary>
    public sealed class ExitConfirmPopupView : PopupView
    {
        private const string LeaveButtonName = "ui_button_popup_exit_leave";
        private const string StayButtonName = "ui_button_popup_exit_stay";
        private const string MessageName = "ui_text_popup_exit_message_value";

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Button _stayButton;
        [SerializeField] private TMP_Text _message;

        public event Action LeaveRequested;
        public event Action StayRequested;

        protected override string PopupId => "exit";

        public void Show(int rewardCount)
        {
            _message.text = rewardCount switch
            {
                0 => "You haven't collected any rewards yet.\nLeave anyway?",
                1 => "Leave now and keep the reward you've collected?",
                _ => $"Leave now and keep the {rewardCount} rewards you've collected?",
            };
            Show();
        }

        private void OnEnable()
        {
            _leaveButton.onClick.AddListener(HandleLeaveClicked);
            _stayButton.onClick.AddListener(HandleStayClicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _leaveButton.onClick.RemoveListener(HandleLeaveClicked);
            _stayButton.onClick.RemoveListener(HandleStayClicked);
        }

        private void HandleLeaveClicked() => LeaveRequested?.Invoke();

        private void HandleStayClicked() => StayRequested?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            transform.AssignIfFound(ref _leaveButton, LeaveButtonName);
            transform.AssignIfFound(ref _stayButton, StayButtonName);
            transform.AssignIfFound(ref _message, MessageName);
        }
    }
}
