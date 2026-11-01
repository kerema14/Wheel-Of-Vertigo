using System;
using System.Collections.Generic;
using NUnit.Framework;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Tests
{
    public class GameSessionTests
    {
        private const int RewardSlice = 0;
        private const int BombSlice = 1;
        private const int BaseAmount = 100;

        private TestZones _zones;
        private ZoneDefinition _normal;
        private ZoneDefinition _safe;
        private ZoneDefinition _super;
        private RewardDefinition _cash;
        private RewardDefinition _unlock;
        private FixedSlicePicker _picker;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _zones = new TestZones();
            _cash = _zones.CreateReward("cash");
            _unlock = _zones.CreateReward("unlock");
            _normal = _zones.Create("normal", 2);
            _safe = _zones.Create("safe", 5);
            _super = _zones.Create("super", 30, _unlock);
            _picker = new FixedSlicePicker();
            var rules = new ZoneRules(new[] { _super, _safe }, _normal, 0.1f);
            _session = new GameSession(rules, new TestWheelFactory(_cash, _normal, rules), _picker);
        }

        [TearDown]
        public void TearDown()
        {
            _zones.DestroyAll();
        }

        [Test]
        public void NewSession_StartsReadyAtZoneOne()
        {
            Assert.AreEqual(1, _session.CurrentZone.Index);
            Assert.AreEqual(SessionState.Ready, _session.State);
            Assert.IsTrue(_session.CanSpin);
        }

        [Test]
        public void WinningSpin_AddsRewardThenAdvancesZone()
        {
            SpinAndResolve(RewardSlice);

            Assert.AreEqual(SessionState.Collecting, _session.State);
            Assert.AreEqual(BaseAmount, _session.Inventory.GetAmount(_cash));

            _session.AdvanceZone();

            Assert.AreEqual(2, _session.CurrentZone.Index);
            Assert.AreEqual(SessionState.Ready, _session.State);
        }

        [Test]
        public void Bomb_ClearsRewardsAndBustsRun()
        {
            PlayWinningZones(3);

            SpinAndResolve(BombSlice);

            Assert.AreEqual(SessionState.Busted, _session.State);
            Assert.AreEqual(0, _session.Inventory.Count);
            Assert.IsFalse(_session.CanSpin);
            Assert.IsFalse(_session.CanLeave);
        }

        [Test]
        public void CanLeave_AtAnyZoneWhileReady()
        {
            Assert.IsTrue(_session.CanLeave, "zone 1 is a normal zone");

            PlayWinningZones(4);
            Assert.AreEqual(5, _session.CurrentZone.Index);
            Assert.IsTrue(_session.CanLeave, "zone 5 is a safe zone");

            _picker.Next = RewardSlice;
            _session.Spin();
            Assert.IsFalse(_session.CanLeave, "can't leave while the wheel is spinning");

            _session.ResolveSpin();
            Assert.IsFalse(_session.CanLeave, "can't leave while the won reward is being collected");

            _session.AdvanceZone();
            Assert.AreEqual(6, _session.CurrentZone.Index);
            Assert.IsTrue(_session.CanLeave, "zone 6 is a normal zone");
        }

        [Test]
        public void CashOut_ReturnsCollectedRewards()
        {
            PlayWinningZones(4);

            IReadOnlyList<RewardStack> rewards = _session.CashOut();

            Assert.AreEqual(SessionState.CashedOut, _session.State);
            Assert.AreEqual(1, rewards.Count);
            Assert.Greater(rewards[0].Amount, 0);
        }

        [Test]
        public void CashOut_ThrowsWhileSpinning()
        {
            _picker.Next = RewardSlice;
            _session.Spin();

            Assert.Throws<InvalidOperationException>(() => _session.CashOut());
        }

        [Test]
        public void Spin_ThrowsWhenNotReady()
        {
            _picker.Next = RewardSlice;
            _session.Spin();

            Assert.Throws<InvalidOperationException>(() => _session.Spin());
        }

        [Test]
        public void SafeAndSuperWheels_HaveNoBomb()
        {
            PlayWinningZones(4);
            Assert.AreSame(_safe, _session.CurrentZone.Definition);
            AssertNoBomb(_session.CurrentZone.Wheel);

            PlayWinningZones(25);
            Assert.AreEqual(30, _session.CurrentZone.Index);
            Assert.AreSame(_super, _session.CurrentZone.Definition);
            AssertNoBomb(_session.CurrentZone.Wheel);
        }

        [Test]
        public void ReachingSuperZone_WaitsForUnlockToBeClaimed()
        {
            PlayWinningZones(28);
            SpinAndResolve(RewardSlice);
            _session.AdvanceZone();

            Assert.AreEqual(30, _session.CurrentZone.Index);
            Assert.AreSame(_unlock, _session.CurrentZone.Unlock);
            Assert.AreEqual(SessionState.Unlocking, _session.State);
            Assert.IsFalse(_session.CanSpin, "can't spin before claiming");
            Assert.IsFalse(_session.CanLeave, "can't leave before claiming");
        }

        [Test]
        public void ClaimUnlock_AddsRewardAndReadiesZone()
        {
            PlayWinningZones(28);
            SpinAndResolve(RewardSlice);
            _session.AdvanceZone();

            RewardDefinition claimed = _session.ClaimUnlock();

            Assert.AreSame(_unlock, claimed);
            Assert.AreEqual(1, _session.Inventory.GetAmount(_unlock));
            Assert.AreEqual(SessionState.Ready, _session.State);
            Assert.IsTrue(_session.CanSpin);
        }

        [Test]
        public void ClaimUnlock_ThrowsWithNothingToUnlock()
        {
            Assert.IsFalse(_session.CurrentZone.HasUnlock);
            Assert.Throws<InvalidOperationException>(() => _session.ClaimUnlock());
        }

        [Test]
        public void ClaimedUnlock_IsLostToTheBomb()
        {
            PlayWinningZones(30);
            Assert.AreEqual(31, _session.CurrentZone.Index);
            Assert.AreEqual(1, _session.Inventory.GetAmount(_unlock));

            SpinAndResolve(BombSlice);

            Assert.AreEqual(0, _session.Inventory.GetAmount(_unlock));
        }

        [Test]
        public void Rewards_GrowWithZone()
        {
            int zoneOneAmount = _session.CurrentZone.Wheel.GetSlice(RewardSlice).Amount;
            PlayWinningZones(1);
            int zoneTwoAmount = _session.CurrentZone.Wheel.GetSlice(RewardSlice).Amount;

            Assert.Greater(zoneTwoAmount, zoneOneAmount);
        }

        [Test]
        public void Restart_ResetsZoneAndRewards()
        {
            PlayWinningZones(2);
            SpinAndResolve(BombSlice);

            _session.Restart();

            Assert.AreEqual(1, _session.CurrentZone.Index);
            Assert.AreEqual(SessionState.Ready, _session.State);
            Assert.AreEqual(0, _session.Inventory.Count);
        }

        [Test]
        public void StateChanged_IsRaisedForEveryTransition()
        {
            var states = new List<SessionState>();
            _session.StateChanged += states.Add;

            SpinAndResolve(RewardSlice);
            _session.AdvanceZone();

            CollectionAssert.AreEqual(
                new[] { SessionState.Spinning, SessionState.Collecting, SessionState.Ready }, states);
        }

        private void SpinAndResolve(int sliceIndex)
        {
            _picker.Next = sliceIndex;
            _session.Spin();
            _session.ResolveSpin();
        }

        /// <summary>Wins <paramref name="count"/> zones in a row, claiming any super zone unlock on the way.</summary>
        private void PlayWinningZones(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpinAndResolve(RewardSlice);
                _session.AdvanceZone();
                if (_session.State == SessionState.Unlocking) _session.ClaimUnlock();
            }
        }

        private static void AssertNoBomb(Wheel wheel)
        {
            foreach (WheelSlice slice in wheel.Slices)
                Assert.IsFalse(slice.IsBomb);
        }

        private sealed class FixedSlicePicker : ISlicePicker
        {
            public int Next { get; set; }
            public int Pick(Wheel wheel) => Next;
        }

        /// <summary>Normal wheels are [reward, bomb]; every other zone type's wheel is [reward, reward].</summary>
        private sealed class TestWheelFactory : IWheelFactory
        {
            private readonly RewardDefinition _reward;
            private readonly ZoneDefinition _normal;
            private readonly ZoneRules _rules;

            public TestWheelFactory(RewardDefinition reward, ZoneDefinition normal, ZoneRules rules)
            {
                _reward = reward;
                _normal = normal;
                _rules = rules;
            }

            public Wheel Create(int zone, ZoneDefinition definition)
            {
                var reward = new WheelSlice(SliceType.Reward, _reward, _rules.ScaleAmount(BaseAmount, zone));
                WheelSlice second = definition == _normal
                    ? new WheelSlice(SliceType.Bomb, null, 0)
                    : reward;
                return new Wheel(null, new[] { reward, second });
            }
        }
    }
}
