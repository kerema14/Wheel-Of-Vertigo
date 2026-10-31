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
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        private GameSession _session;
        private WheelConfig _shownWheel;
        private bool _isAnimating;   // true while a presentation step must finish before the player acts again

        private void Awake()
        {
            ZoneRules rules = _config.CreateRules();
            _session = new GameSession(rules, new WheelFactory(_config, rules), new RandomSlicePicker(_config.Seed));
            _zoneBarView.Initialize(rules);
        }

        private void OnEnable()
        {
            _session.StateChanged += OnStateChanged;
            _wheelView.SpinRequested += OnSpinRequested;
            _rewardPanelView.ExitRequested += OnExitRequested;
            _bombPopupView.RestartRequested += OnRestartRequested;
            _cashOutPopupView.CollectRequested += OnCollectRequested;
        }

        private void OnDisable()
        {
            _session.StateChanged -= OnStateChanged;
            _wheelView.SpinRequested -= OnSpinRequested;
            _rewardPanelView.ExitRequested -= OnExitRequested;
            _bombPopupView.RestartRequested -= OnRestartRequested;
            _cashOutPopupView.CollectRequested -= OnCollectRequested;
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
            ShowZone(animate: true);
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

            _cashOutPopupView.Show(_session.CashOut());
        }

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

        private void ShowZone(bool animate)
        {
            Zone zone = _session.CurrentZone;
            ZoneRules rules = _session.Rules;

            _zoneBarView.ShowZone(zone.Index, animate);
            _zoneInfoView.Show(
                rules.NextZoneOfType(zone.Index, ZoneType.Safe),
                rules.NextZoneOfType(zone.Index, ZoneType.Super),
                zone.Type);

            if (_shownWheel == null)
            {
                BindWheel(zone);
            }
            else if (_shownWheel != zone.Wheel.Config)
            {
                SetAnimating(true);
                _wheelView.PlayTierChange(() => BindWheel(zone), () => SetAnimating(false));
            }
            else
            {
                BindWheel(zone);
                if (animate) _wheelView.PlayRefresh();
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
