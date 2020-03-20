using System.Collections.Generic;
using NUnit.Framework;
using Poker;
using Poker.Players;

namespace Tests
{
    public class PotTests
    {
        [Test]
        public void CalculateSidePots_Calculation_ReturnsCorrectSidePots()
        {
            List<TablePlayer> players = new List<TablePlayer>
            {
                new TablePlayer("f1", 0, null, 0, 0, null, null) {TotalBet = 600, Folded = true},
                null,
                new TablePlayer("f2", 0, null, 0, 0, null, null) {TotalBet = 400, Folded = false},
                new TablePlayer("f3", 0, null, 0, 0, null, null) {TotalBet = 300, Folded = false},
                new TablePlayer("f4", 0, null, 0, 0, null, null) {TotalBet = 400, Folded = false},
                null,
                null,
                new TablePlayer("f5", 0, null, 0, 0, null, null) {TotalBet = 600, Folded = false},
                new TablePlayer("f6", 0, null, 0, 0, null, null) {TotalBet = 400, Folded = false},
                null
            };

            var sidePots = Pot.CalculateSidePots(players);
        }
    }
}
