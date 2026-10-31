using System;
using NUnit.Framework;
using UnityEngine;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Tests
{
    public class RewardInventoryTests
    {
        private RewardDefinition _cash;
        private RewardDefinition _gold;
        private RewardInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _cash = ScriptableObject.CreateInstance<RewardDefinition>();
            _gold = ScriptableObject.CreateInstance<RewardDefinition>();
            _inventory = new RewardInventory();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_cash);
            UnityEngine.Object.DestroyImmediate(_gold);
        }

        [Test]
        public void Add_StacksSameReward()
        {
            _inventory.Add(_cash, 100);
            int total = _inventory.Add(_cash, 50);

            Assert.AreEqual(150, total);
            Assert.AreEqual(150, _inventory.GetAmount(_cash));
            Assert.AreEqual(1, _inventory.Count);
        }

        [Test]
        public void Snapshot_KeepsCollectionOrder()
        {
            _inventory.Add(_gold, 1);
            _inventory.Add(_cash, 100);
            _inventory.Add(_gold, 2);

            var stacks = _inventory.Snapshot();

            Assert.AreEqual(2, stacks.Count);
            Assert.AreSame(_gold, stacks[0].Reward);
            Assert.AreEqual(3, stacks[0].Amount);
            Assert.AreSame(_cash, stacks[1].Reward);
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            _inventory.Add(_cash, 100);
            _inventory.Clear();

            Assert.AreEqual(0, _inventory.Count);
            Assert.AreEqual(0, _inventory.GetAmount(_cash));
        }

        [Test]
        public void Add_RejectsNonPositiveAmountsAndNullRewards()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Add(_cash, 0));
            Assert.Throws<ArgumentNullException>(() => _inventory.Add(null, 10));
        }
    }
}
