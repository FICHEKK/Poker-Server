using NUnit.Framework;

namespace Tests
{
    public class EloSystemTests
    {
        [TestCase(1000, 1000, 0.5000)]
        [TestCase(1200, 1000, 0.7597)]
        [TestCase(1000, 1200, 0.2402)]
        [TestCase(2000, 1000, 0.9968)]
        public void CalculateExpectedScore_Calculation_ReturnsCorrectValues(int ratingA, int ratingB, double validValue)
        {
            double expectedScore = EloSystem.CalculateExpectedScore(ratingA, ratingB);
            Assert.AreEqual(validValue, expectedScore, 0.001);
        }
    }
}
