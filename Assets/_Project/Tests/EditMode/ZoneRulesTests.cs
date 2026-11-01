using System;
using NUnit.Framework;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Tests
{
    public class ZoneRulesTests
    {
        private ZoneRules _rules;

        [SetUp]
        public void SetUp() => _rules = new ZoneRules(5, 30, 0.1f);

        [TestCase(1, ZoneType.Normal)]
        [TestCase(4, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(6, ZoneType.Normal)]
        [TestCase(10, ZoneType.Safe)]
        [TestCase(25, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]
        [TestCase(35, ZoneType.Safe)]
        [TestCase(60, ZoneType.Super)]
        public void GetZoneType_FollowsSafeAndSuperIntervals(int zone, ZoneType expected)
        {
            Assert.AreEqual(expected, _rules.GetZoneType(zone));
        }

        [TestCase(1000, 1, 1000)]
        [TestCase(1000, 2, 1100)]
        [TestCase(1000, 11, 2000)]
        [TestCase(1, 1, 1)]
        public void ScaleAmount_GrowsLinearlyFromZoneOne(int baseAmount, int zone, int expected)
        {
            Assert.AreEqual(expected, _rules.ScaleAmount(baseAmount, zone));
        }

        [TestCase(1, ZoneType.Safe, 5)]
        [TestCase(5, ZoneType.Safe, 5)]
        [TestCase(6, ZoneType.Safe, 10)]
        [TestCase(26, ZoneType.Safe, 35)]   // 30 is a super zone, not a safe one
        [TestCase(1, ZoneType.Super, 30)]
        [TestCase(30, ZoneType.Super, 30)]
        [TestCase(31, ZoneType.Super, 60)]
        [TestCase(5, ZoneType.Normal, 6)]
        public void NextZoneOfType_FindsFirstMatchingZoneFromGivenZone(int from, ZoneType type, int expected)
        {
            Assert.AreEqual(expected, _rules.NextZoneOfType(from, type));
        }

        [Test]
        public void Constructor_RejectsIntervalsWhereEverySafeZoneIsSuper()
        {
            Assert.Throws<ArgumentException>(() => new ZoneRules(30, 30, 0f));
        }
    }
}
