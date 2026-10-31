using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>Shown when the wheel lands on the bomb. Everything collected is gone; the only way on is to restart.</summary>
    public sealed class BombPopupView : PopupView
    {
        private const string RestartButtonName = "ui_button_popup_bomb_restart";
        private const string IconName = "ui_image_popup_bomb_icon";

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private RectTransform _icon;

        public event Action RestartRequested;

        protected override string PopupId => "bomb";

        protected override void OnShow()
        {
            _icon.DOKill(true);
            _icon.DOShakeRotation(0.6f, new Vector3(0f, 0f, 25f), 12).SetDelay(0.2f);
        }

        private void OnEnable() => _restartButton.onClick.AddListener(HandleRestartClicked);

        protected override void OnDisable()
        {
            base.OnDisable();
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
            _icon.DOKill(true);
        }

        private void HandleRestartClicked() => RestartRequested?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            transform.AssignIfFound(ref _restartButton, RestartButtonName);
            transform.AssignIfFound(ref _icon, IconName);
        }
    }
}
