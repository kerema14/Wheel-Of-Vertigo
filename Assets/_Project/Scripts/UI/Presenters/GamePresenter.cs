using System;
using UnityEngine;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Views;

namespace WheelOfVertigo.UI.Presenters
{
    /// <summary>
    /// Composition root and flow coordinator. It owns the GameSession, turns view events into session calls,
    /// and plays the matching animations in order. Views never talk to the session, and the session never
    /// knows about views.
    /// </summary>
    public sealed class GamePresenter : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;

        [Header("Views")]
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private ZoneInfoView _zoneInfoView;
        [SerializeField] private RewardPanelView _rewardPanelView;
        [SerializeField] private RewardFlyAnimator _rewardFlyAnimator;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private ExitConfirmPopupView _exitConfirmPopupView;
        [SerializeField] private UnlockPopupView _unlockPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        private GameSession _session;
        private WheelConfig _shownWheel;
        private bool _isAnimating;   // true while a presentation step must finish before the player acts again

        private void Awake()
        {
            ZoneRules rules = _config.CreateRules();
            _session = new GameSession(rules, new WheelFactory(rules), new RandomSlicePicker(_config.Seed));
            _zoneBarView.Initialize(rules);
            _zoneInfoView.Initialize(rules);
        }

        private void OnEnable()
        {
            _session.StateChanged += OnStateChanged;
            _wheelView.SpinRequested += OnSpinRequested;
            _rewardPanelView.ExitRequested += OnExitRequested;
            _bombPopupView.RestartRequested += OnRestartRequested;
            _exitConfirmPopupView.LeaveRequested += OnLeaveConfirmed;
            _exitConfirmPopupView.StayRequested += OnStayRequested;
            _cashOutPopupView.CollectRequested += OnCollectRequested;
            _unlockPopupView.ClaimRequested += OnClaimRequested;
        }

        private void OnDisable()
        {
            _session.StateChanged -= OnStateChanged;
            _wheelView.SpinRequested -= OnSpinRequested;
            _rewardPanelView.ExitRequested -= OnExitRequested;
            _bombPopupView.RestartRequested -= OnRestartRequested;
            _exitConfirmPopupView.LeaveRequested -= OnLeaveConfirmed;
            _exitConfirmPopupView.StayRequested -= OnStayRequested;
            _cashOutPopupView.CollectRequested -= OnCollectRequested;
            _unlockPopupView.ClaimRequested -= OnClaimRequested;
        }

        private void Start() => ShowNewRun();

        private void OnSpinRequested()
        {
            if (!CanAct(_session.CanSpin)) return;

            SpinResult result = _session.Spin();
            _wheelView.PlaySpin(result.SliceIndex, OnSpinStopped);
        }

        private void OnSpinStopped()
        {
            SpinResult result = _session.ResolveSpin();

            if (result.IsBomb)
                PlayBomb(result);
            else
                PlayReward(result);
        }

        private void PlayReward(SpinResult result)
        {
            RewardDefinition reward = result.Slice.Reward;
            int total = _session.Inventory.GetAmount(reward);
            RewardEntryView entry = _rewardPanelView.GetOrCreateEntry(reward);

            _wheelView.PlayWin(result.SliceIndex);
            _rewardFlyAnimator.Play(
                reward.Icon,
                result.Slice.Amount,
                _wheelView.GetSliceIcon(result.SliceIndex),
                entry.IconRect,
                entry.Punch,
                () =>
                {
                    entry.AnimateAmount(total);
                    AdvanceZone();
                });
        }

        private void AdvanceZone()
        {
            _session.AdvanceZone();
            ShowZone(animate: true, onShown: ShowUnlockIfAny);
        }

        // A zone with a guaranteed reward presents it once the bar and the wheel have settled on the zone.
        private void ShowUnlockIfAny()
        {
            if (_session.State == SessionState.Unlocking)
                _unlockPopupView.Show(_session.CurrentZone.Unlock);
        }

        private void OnClaimRequested()
        {
            if (_session.State != SessionState.Unlocking) return;

            SetAnimating(true);
            RewardDefinition reward = _session.ClaimUnlock();
            int total = _session.Inventory.GetAmount(reward);
            RewardEntryView entry = _rewardPanelView.GetOrCreateEntry(reward);

            _unlockPopupView.Hide(() => _rewardFlyAnimator.Play(
                reward.Icon,
                1,
                _unlockPopupView.IconRect,
                entry.IconRect,
                entry.Punch,
                () =>
                {
                    entry.AnimateAmount(total);
                    SetAnimating(false);
                }));
        }

        private void PlayBomb(SpinResult result)
        {
            _wheelView.PlayBomb(result.SliceIndex, () =>
            {
                _rewardPanelView.Clear();
                _bombPopupView.Show();
            });
        }

        private void OnExitRequested()
        {
            if (!CanAct(_session.CanLeave)) return;

            _exitConfirmPopupView.Show(_session.Inventory.Count);
        }

        private void OnLeaveConfirmed() => _exitConfirmPopupView.Hide(() =>
        {
            if (_session.CanLeave)
                _cashOutPopupView.Show(_session.CashOut());
        });

        private void OnStayRequested() => _exitConfirmPopupView.Hide();

        private void OnRestartRequested() => _bombPopupView.Hide(RestartRun);

        private void OnCollectRequested() => _cashOutPopupView.Hide(RestartRun);

        private void RestartRun()
        {
            _session.Restart();
            ShowNewRun();
        }

        private void ShowNewRun()
        {
            _rewardPanelView.Clear();
            ShowZone(animate: false);
            RefreshInteractivity();
        }

        private void ShowZone(bool animate, Action onShown = null)
        {
            Zone zone = _session.CurrentZone;
            _zoneBarView.ShowZone(zone.Index, animate);
            _zoneInfoView.Show(zone.Index);

            if (_shownWheel == null)
            {
                BindWheel(zone);
                onShown?.Invoke();
            }
            else if (_shownWheel != zone.Wheel.Config)
            {
                SetAnimating(true);
                _wheelView.PlayTierChange(() => BindWheel(zone), () =>
                {
                    SetAnimating(false);
                    onShown?.Invoke();
                });
            }
            else
            {
                BindWheel(zone);
                if (animate) _wheelView.PlayRefresh();
                onShown?.Invoke();
            }
        }

        private void BindWheel(Zone zone)
        {
            _shownWheel = zone.Wheel.Config;
            _wheelView.Bind(zone.Wheel, _config.BombIcon);
        }

        private bool CanAct(bool sessionAllows) => sessionAllows && !_isAnimating;

        private void SetAnimating(bool animating)
        {
            _isAnimating = animating;
            RefreshInteractivity();
        }

        private void OnStateChanged(SessionState state) => RefreshInteractivity();

        private void RefreshInteractivity()
        {
            _wheelView.SetSpinInteractable(CanAct(_session.CanSpin));
            _rewardPanelView.SetExitInteractable(CanAct(_session.CanLeave));
        }
    }
}
