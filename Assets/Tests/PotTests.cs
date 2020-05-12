using System.Collections.Generic;
using NUnit.Framework;
using Poker;
using Poker.Players;

namespace Tests
{
    public class PotTests
    {
        [Test]
        public void MergeAllRedundantSidePots()
        {
            var A = new TablePlayer(null, 0, null, 0);
            var B = new TablePlayer(null, 0, null, 0);
            var C = new TablePlayer(null, 0, null, 0);
            var D = new TablePlayer(null, 0, null, 0);

            A.Index = 0;
            B.Index = 1;
            C.Index = 2;
            D.Index = 3;

            A.Folded = true;
            B.Folded = true;
            C.Folded = true;
            D.Folded = false;

            A.TotalBet = 20;
            B.TotalBet = 40;
            C.TotalBet = 40;
            D.TotalBet = 50;

            var sidePots = Pot.CalculateSidePots(new List<TablePlayer> {A, B, C, D});
        }
    }
}
