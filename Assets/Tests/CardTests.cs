using NUnit.Framework;

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
    }
}
