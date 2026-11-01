#if UNITY_EDITOR || DEVELOPMENT_BUILD
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;
using WheelOfVertigo.UI.Cheats;
using WheelOfVertigo.UI.Views;

namespace WheelOfVertigo.UI.Presenters
{
    /// <summary>
    /// What the cheat menu can do to a run. Every cheat goes through the session's normal API, so the rules
    /// still hold: skipping a zone wins it, and a forced spin lands like any other spin.
    /// This file only exists in the Editor and development builds.
    /// </summary>
    public sealed partial class GamePresenter
    {
        private CheatSlicePicker _cheatPicker;

        internal GameSession CheatSession => _session;

        /// <summary>True when the wheel is idle and nothing is animating, the same moments the player could spin.</summary>
        internal bool CheatCanAct => CanAct(_session.CanSpin);

        internal int? CheatForcedSlice
        {
            get => _cheatPicker.Forced;
            set => _cheatPicker.Forced = value;
        }

        partial void SetUpCheats(ref ISlicePicker picker)
        {
            _cheatPicker = new CheatSlicePicker(picker);
            picker = _cheatPicker;
            gameObject.AddComponent<CheatMenu>().Initialize(this);
        }

        /// <summary>
        /// Wins every zone up to <paramref name="zone"/> instantly, claiming unlocks on the way. The target zone's
        /// own unlock, if it has one, is presented as usual.
        /// </summary>
        internal void CheatSkipTo(int zone)
        {
            if (!CheatCanAct || zone <= _session.CurrentZone.Index) return;

            int? forced = _cheatPicker.Forced;
            while (_session.CurrentZone.Index < zone)
            {
                if (_session.State == SessionState.Unlocking) _session.ClaimUnlock();

                _cheatPicker.Forced = FirstRewardSlice(_session.CurrentZone.Wheel);
                _session.Spin();
                _session.ResolveSpin();
                _session.AdvanceZone();
            }
            _cheatPicker.Forced = forced;

            ShowInventory();
            ShowZone(animate: false, onShown: ShowUnlockIfAny);
            RefreshInteractivity();
        }

        /// <summary>Adds every reward on the current wheel, at its slice amount, to fill the reward list quickly.</summary>
        internal void CheatAddWheelRewards()
        {
            if (!CheatCanAct) return;

            foreach (WheelSlice slice in _session.CurrentZone.Wheel.Slices)
            {
                if (slice.IsBomb) continue;

                int total = _session.Inventory.Add(slice.Reward, slice.Amount);
                _rewardPanelView.GetOrCreateEntry(slice.Reward).AnimateAmount(total);
            }
        }

        private void ShowInventory()
        {
            _rewardPanelView.Clear();
            foreach (RewardStack stack in _session.Inventory.Snapshot())
            {
                RewardEntryView entry = _rewardPanelView.GetOrCreateEntry(stack.Reward);
                entry.Bind(stack.Reward.Icon, stack.Amount);
            }
        }

        private static int FirstRewardSlice(Wheel wheel)
        {
            for (int i = 0; i < wheel.SliceCount; i++)
                if (!wheel.GetSlice(i).IsBomb)
                    return i;

            return 0;
        }
    }
}
#endif
