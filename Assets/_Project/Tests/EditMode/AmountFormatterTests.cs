using NUnit.Framework;
using WheelOfVertigo.Core.Utils;

namespace WheelOfVertigo.Tests
{
    public class AmountFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(950, "950")]
        [TestCase(1000, "1K")]
        [TestCase(1200, "1.2K")]
        [TestCase(1999, "1.9K")]
        [TestCase(10000, "10K")]
        [TestCase(999999, "999.9K")]
        [TestCase(3400000, "3.4M")]
        public void Format_ShortensLargeAmounts(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.Format(amount));
        }

        [Test]
        public void FormatMultiplier_PrefixesWithX()
        {
            Assert.AreEqual("x1.5K", AmountFormatter.FormatMultiplier(1500));
        }
    }
}
