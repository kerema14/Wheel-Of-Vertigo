using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.UI.Views
{
    /// <summary>Shown on reaching a super zone: presents the zone's guaranteed reward until the player claims it.</summary>
    public sealed class UnlockPopupView : PopupView
    {
        private const string ClaimButtonName = "ui_button_popup_unlock_claim";
        private const string IconName = "ui_image_popup_unlock_icon_value";
        private const string NameTextName = "ui_text_popup_unlock_name_value";
        private const string GlowName = "ui_image_popup_unlock_glow";

        [SerializeField, Min(1f)] private float _glowTurnDuration = 12f;

        [Header("References (set from the hierarchy in OnValidate)")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private RectTransform _glow;

        public event Action ClaimRequested;

        /// <summary>Where the claimed reward flies from.</summary>
        public RectTransform IconRect => _icon.rectTransform;

        protected override string PopupId => "unlock";

        public void Show(RewardDefinition reward)
        {
            _icon.sprite = reward.Icon;
            _nameText.text = reward.DisplayName.ToUpperInvariant();
            Show();
        }

        protected override void OnShow()
        {
            RectTransform icon = _icon.rectTransform;
            icon.DOKill(true);
            icon.localScale = Vector3.one * 0.6f;
            icon.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.15f);

            _glow.DOKill();
            _glow.localRotation = Quaternion.identity;
            _glow.DOLocalRotate(new Vector3(0f, 0f, -360f), _glowTurnDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void OnEnable() => _claimButton.onClick.AddListener(HandleClaimClicked);

        protected override void OnDisable()
        {
            base.OnDisable();
            _claimButton.onClick.RemoveListener(HandleClaimClicked);
            _icon.rectTransform.DOKill(true);
            _glow.DOKill();
        }

        private void HandleClaimClicked() => ClaimRequested?.Invoke();

        protected override void OnValidate()
        {
            base.OnValidate();
            transform.AssignIfFound(ref _claimButton, ClaimButtonName);
            transform.AssignIfFound(ref _icon, IconName);
            transform.AssignIfFound(ref _nameText, NameTextName);
            transform.AssignIfFound(ref _glow, GlowName);
        }
    }
}
