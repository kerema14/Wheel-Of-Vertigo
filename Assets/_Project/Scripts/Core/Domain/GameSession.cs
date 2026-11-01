using System;
using System.Collections.Generic;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Core.Domain
{
    public enum SessionState
    {
        Ready,        // waiting for the player to spin or leave
        Spinning,     // a slice has been picked and the wheel is turning
        Collecting,   // a reward was won; the zone advances once the reward animation is done
        Unlocking,    // reached a super zone; its guaranteed reward is waiting to be claimed
        Busted,       // hit the bomb; everything collected is lost
        CashedOut     // left the run with the rewards
    }

    public readonly struct SpinResult
    {
        public int SliceIndex { get; }
        public WheelSlice Slice { get; }
        public bool IsBomb => Slice.IsBomb;

        public SpinResult(int sliceIndex, WheelSlice slice)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
        }
    }

    /// <summary>
    /// One run of the game as a small state machine. It knows nothing about Unity or animation,
    /// so the presenter decides when each step happens and the rules stay unit-testable.
    /// </summary>
    public sealed class GameSession
    {
        private readonly IWheelFactory _wheelFactory;
        private readonly ISlicePicker _picker;
        private readonly ISuperZoneRewards _superZoneRewards;
        private SpinResult _pendingResult;

        public event Action<SessionState> StateChanged;

        public ZoneRules Rules { get; }
        public RewardInventory Inventory { get; } = new RewardInventory();
        public Zone CurrentZone { get; private set; }
        public SessionState State { get; private set; }

        public bool CanSpin => State == SessionState.Ready;
        // As in Critical Strike, the player may leave at any zone, as long as the wheel isn't spinning.
        public bool CanLeave => State == SessionState.Ready;

        public GameSession(ZoneRules rules, IWheelFactory wheelFactory, ISlicePicker picker, ISuperZoneRewards superZoneRewards)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _wheelFactory = wheelFactory ?? throw new ArgumentNullException(nameof(wheelFactory));
            _picker = picker ?? throw new ArgumentNullException(nameof(picker));
            _superZoneRewards = superZoneRewards ?? throw new ArgumentNullException(nameof(superZoneRewards));
            CurrentZone = CreateZone(ZoneRules.FirstZone);
            State = SessionState.Ready;
        }

        /// <summary>Picks the slice the wheel will land on. The result is applied by <see cref="ResolveSpin"/>.</summary>
        public SpinResult Spin()
        {
            EnsureState(SessionState.Ready, nameof(Spin));

            int index = _picker.Pick(CurrentZone.Wheel);
            _pendingResult = new SpinResult(index, CurrentZone.Wheel.GetSlice(index));
            SetState(SessionState.Spinning);
            return _pendingResult;
        }

        /// <summary>Called when the wheel stops: grants the reward or busts the run.</summary>
        public SpinResult ResolveSpin()
        {
            EnsureState(SessionState.Spinning, nameof(ResolveSpin));

            if (_pendingResult.IsBomb)
            {
                Inventory.Clear();
                SetState(SessionState.Busted);
            }
            else
            {
                Inventory.Add(_pendingResult.Slice.Reward, _pendingResult.Slice.Amount);
                SetState(SessionState.Collecting);
            }

            return _pendingResult;
        }

        /// <summary>
        /// Moves to the next zone once the won reward has been shown. A zone with a guaranteed reward
        /// waits in <see cref="SessionState.Unlocking"/> until <see cref="ClaimUnlock"/> is called.
        /// </summary>
        public Zone AdvanceZone()
        {
            EnsureState(SessionState.Collecting, nameof(AdvanceZone));

            CurrentZone = CreateZone(CurrentZone.Index + 1);
            SetState(CurrentZone.HasUnlock ? SessionState.Unlocking : SessionState.Ready);
            return CurrentZone;
        }

        /// <summary>Adds the current zone's guaranteed reward to the run. Like any reward, a later bomb takes it.</summary>
        public RewardDefinition ClaimUnlock()
        {
            EnsureState(SessionState.Unlocking, nameof(ClaimUnlock));

            Inventory.Add(CurrentZone.Unlock, 1);
            SetState(SessionState.Ready);
            return CurrentZone.Unlock;
        }

        /// <summary>Leaves the run and keeps the rewards. Only allowed while the wheel isn't spinning.</summary>
        public IReadOnlyList<RewardStack> CashOut()
        {
            if (!CanLeave)
                throw new InvalidOperationException($"Can't leave at zone {CurrentZone.Index} while {State}.");

            IReadOnlyList<RewardStack> rewards = Inventory.Snapshot();
            SetState(SessionState.CashedOut);
            return rewards;
        }

        public void Restart()
        {
            Inventory.Clear();
            CurrentZone = CreateZone(ZoneRules.FirstZone);
            SetState(SessionState.Ready);
        }

        private Zone CreateZone(int index)
        {
            ZoneType type = Rules.GetZoneType(index);
            return new Zone(index, type, _wheelFactory.Create(index, type), _superZoneRewards.GetReward(index));
        }

        private void EnsureState(SessionState expected, string action)
        {
            if (State != expected)
                throw new InvalidOperationException($"{action} needs the session to be {expected}, but it is {State}.");
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
