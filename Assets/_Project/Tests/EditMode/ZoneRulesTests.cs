using System;
using NUnit.Framework;
using WheelOfVertigo.Core.Data;
using WheelOfVertigo.Core.Domain;

namespace WheelOfVertigo.Tests
{
    public class ZoneRulesTests
    {
        private TestZones _zones;
        private ZoneDefinition _normal;
        private ZoneDefinition _safe;
        private ZoneDefinition _super;
        private RewardDefinition _rifle;
        private RewardDefinition _knife;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _zones = new TestZones();
            _rifle = _zones.CreateReward("rifle");
            _knife = _zones.CreateReward("knife");
            _normal = _zones.Create("normal", 2);
            _safe = _zones.Create("safe", 5);
            _super = _zones.Create("super", 30, _rifle, _knife);
            _rules = new ZoneRules(new[] { _super, _safe }, _normal, 0.1f);
        }

        [TearDown]
        public void TearDown() => _zones.DestroyAll();

        [TestCase(1, "normal")]
        [TestCase(4, "normal")]
        [TestCase(5, "safe")]
        [TestCase(6, "normal")]
        [TestCase(10, "safe")]
        [TestCase(25, "safe")]
        [TestCase(30, "super")]   // also a multiple of 5, but super comes first in the list
        [TestCase(35, "safe")]
        [TestCase(60, "super")]
        public void GetZone_TakesFirstTypeWhoseIntervalDividesTheZone(int zone, string expected)
        {
            Assert.AreEqual(expected, _rules.GetZone(zone).name);
        }

        [TestCase(1, 1)]
        [TestCase(4, 4)]
        [TestCase(6, 5)]     // 5 is safe, so 6 is the fifth normal zone
        [TestCase(5, 1)]
        [TestCase(25, 5)]
        [TestCase(35, 6)]    // 30 is super, so 35 is only the sixth safe zone
        [TestCase(30, 1)]
        [TestCase(90, 3)]
        public void GetOrdinal_CountsOnlyZonesOfTheSameType(int zone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetOrdinal(zone));
        }

        [Test]
        public void GetUnlock_HandsOutTheTypesUnlocksInOrderAndRepeats()
        {
            Assert.AreSame(_rifle, _rules.GetUnlock(30));
            Assert.AreSame(_knife, _rules.GetUnlock(60));
            Assert.AreSame(_rifle, _rules.GetUnlock(90));
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(29)]
        [TestCase(31)]
        public void GetUnlock_IsNullForTypesWithoutUnlocks(int zone)
        {
            Assert.IsNull(_rules.GetUnlock(zone));
        }

        [TestCase(1, "safe", 5)]
        [TestCase(5, "safe", 5)]
        [TestCase(6, "safe", 10)]
        [TestCase(26, "safe", 35)]   // 30 is a super zone, not a safe one
        [TestCase(1, "super", 30)]
        [TestCase(30, "super", 30)]
        [TestCase(31, "super", 60)]
        [TestCase(5, "normal", 6)]
        [TestCase(29, "normal", 29)]
        public void NextZoneOf_FindsFirstZoneOfTheTypeFromGivenZone(int from, string type, int expected)
        {
            ZoneDefinition definition = type == "safe" ? _safe : type == "super" ? _super : _normal;

            Assert.AreEqual(expected, _rules.NextZoneOf(from, definition));
        }

        [Test]
        public void NewZoneType_NeedsOnlyANewDefinition()
        {
            RewardDefinition crown = _zones.CreateReward("crown");
            ZoneDefinition mega = _zones.Create("mega", 90, crown);   // every third super zone
            var rules = new ZoneRules(new[] { mega, _super, _safe }, _normal, 0.1f);

            Assert.AreSame(mega, rules.GetZone(90));
            Assert.AreSame(_safe, rules.GetZone(95));
            Assert.AreSame(_super, rules.GetZone(120));
            Assert.AreSame(crown, rules.GetUnlock(180));
            Assert.AreEqual(90, rules.NextZoneOf(1, mega));
            Assert.AreEqual(4, rules.GetOrdinal(150), "90 is mega, so 150 is only the fourth super zone");
        }

        [TestCase(1000, 1, 1000)]
        [TestCase(1000, 2, 1100)]
        [TestCase(1000, 11, 2000)]
        [TestCase(1, 1, 1)]
        public void ScaleAmount_GrowsLinearlyFromZoneOne(int baseAmount, int zone, int expected)
        {
            Assert.AreEqual(expected, _rules.ScaleAmount(baseAmount, zone));
        }

        [Test]
        public void Constructor_RejectsATypeThatAnEarlierTypeAlwaysClaims()
        {
            // Every multiple of 10 is a multiple of 5, so with safe first the 10s would never appear.
            ZoneDefinition tens = _zones.Create("tens", 10);

            Assert.Throws<ArgumentException>(() => new ZoneRules(new[] { _safe, tens }, _normal, 0f));
            Assert.DoesNotThrow(() => new ZoneRules(new[] { tens, _safe }, _normal, 0f));
        }

        [Test]
        public void Constructor_RejectsMissingDefaultOrEmptyEntries()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneRules(new[] { _safe }, null, 0f));
            Assert.Throws<ArgumentException>(() => new ZoneRules(new[] { _safe, null }, _normal, 0f));
        }

        [Test]
        public void NextZoneOf_RejectsATypeThatIsNotInTheRules()
        {
            ZoneDefinition stranger = _zones.Create("stranger", 7);

            Assert.Throws<ArgumentException>(() => _rules.NextZoneOf(1, stranger));
        }
    }
}
