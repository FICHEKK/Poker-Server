using System;
using NUnit.Framework;
using Poker.Cards;

namespace Tests
{
    public class CardTests
    {
        [Test]
        public void CompareTo_FirstCardHigherRank_ReturnsOne()
        {
            Card card1 = new Card(Rank.Three, Suit.Heart);
            Card card2 = new Card(Rank.Two, Suit.Club);

            int comparisonResult = card1.CompareTo(card2);

            Assert.AreEqual(1, comparisonResult);
        }

        [Test]
        public void CompareTo_EqualRank_ReturnsZero()
        {
            Card card1 = new Card(Rank.Ace, Suit.Heart);
            Card card2 = new Card(Rank.Ace, Suit.Club);

            int comparisonResult = card1.CompareTo(card2);

            Assert.AreEqual(0, comparisonResult);
        }

        [Test]
        public void CompareTo_FirstCardLowerRank_ReturnsNegativeOne()
        {
            Card card1 = new Card(Rank.Two, Suit.Heart);
            Card card2 = new Card(Rank.Three, Suit.Club);

            int comparisonResult = card1.CompareTo(card2);

            Assert.AreEqual(-1, comparisonResult);
        }

        [Test]
        public void CompareTo_ArgumentNull_ReturnsOne()
        {
            Card card = new Card(Rank.Two, Suit.Heart);

            int comparisonResult = card.CompareTo(null);

            Assert.AreEqual(1, comparisonResult);
        }

        [Test]
        public void CompareTo_SameReference_ReturnsZero()
        {
            Card card = new Card(Rank.Two, Suit.Heart);

            int comparisonResult = card.CompareTo(card);

            Assert.AreEqual(0, comparisonResult);
        }

        [Test]
        public void Parse_ArgumentNull_Throws()
        {
            Assert.Throws<FormatException>(() => Card.Parse(null));
        }

        [TestCase("2H")]
        [TestCase("10D")]
        [TestCase("AS")]
        public void Parse_ValidStringPassed_DoesNotThrow(string s)
        {
            Assert.DoesNotThrow(() => Card.Parse(s));
        }

        [Test]
        public void Parse_ValidTwoSymbolStringPassed_ReturnsValidCard()
        {
            Card parsed = Card.Parse("2H");

            Assert.IsTrue(parsed.Rank == Rank.Two);
            Assert.IsTrue(parsed.Suit == Suit.Heart);
        }


        [Test]
        public void Parse_ValidThreeSymbolStringPassed_ReturnsValidCard()
        {
            Card parsed = Card.Parse("10C");

            Assert.IsTrue(parsed.Rank == Rank.Ten);
            Assert.IsTrue(parsed.Suit == Suit.Club);
        }
    }
}
