using NUnit.Framework;
using UnityEngine;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Tests
{
    public class SuperZoneRewardTableTests
    {
        private RewardDefinition _first;
        private RewardDefinition _second;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _first = ScriptableObject.CreateInstance<RewardDefinition>();
            _second = ScriptableObject.CreateInstance<RewardDefinition>();
            _rules = new ZoneRules(5, 30, 0.1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
        }

        [Test]
        public void SuperZones_UnlockRewardsInOrderAndRepeat()
        {
            var table = new SuperZoneRewardTable(new[] { _first, _second }, _rules);

            Assert.AreSame(_first, table.GetReward(30));
            Assert.AreSame(_second, table.GetReward(60));
            Assert.AreSame(_first, table.GetReward(90));
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(29)]
        [TestCase(31)]
        public void OtherZones_UnlockNothing(int zone)
        {
            var table = new SuperZoneRewardTable(new[] { _first, _second }, _rules);

            Assert.IsNull(table.GetReward(zone));
        }

        [Test]
        public void EmptyList_UnlocksNothing()
        {
            var table = new SuperZoneRewardTable(new RewardDefinition[0], _rules);

            Assert.IsNull(table.GetReward(30));
        }
    }
}
