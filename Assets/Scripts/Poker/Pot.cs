using System;
using System.Collections.Generic;
using System.Linq;
using Poker.Players;

namespace Poker
{
    public sealed class Pot
    {
        public int Value { get; }
        public List<TablePlayer> Contenders { get; }

        private Pot(int value, List<TablePlayer> contenders)
        {
            Value = value;
            Contenders = contenders;
        }

        public static List<Pot> CalculateSidePots(List<TablePlayer> tablePlayers)
        {
            if(tablePlayers == null) throw new ArgumentNullException(nameof(tablePlayers));
            
            var sidePots = new List<Pot>();
            var players = tablePlayers.Where(p => p != null).ToArray();

            while (players.Length > 0)
            {
                int minTotalBet = players.Select(player => player.TotalBet).Min();

                var sidePotValue = minTotalBet * players.Length;
                var contenders = new List<TablePlayer>(players.Where(p => !p.Folded));
                sidePots.Add(new Pot(sidePotValue, contenders));

                foreach (var player in players)
                    player.TotalBet -= minTotalBet;

                players = players.Where(p => p.TotalBet > 0).ToArray();
            }

            return MergeAllRedundantSidePots(sidePots);
        }

        private static List<Pot> MergeAllRedundantSidePots(List<Pot> sidePots)
        {
            if (sidePots.Count <= 1) return sidePots;
            
            var mergedSidePots = new List<Pot>();
            var start = 0;

            while (start < sidePots.Count)
            {
                var count = sidePots[start].Contenders.Count;

                var border = start + 1;
                while (border < sidePots.Count)
                {
                    if(sidePots[border].Contenders.Count < count) break;
                    border++;
                }

                var totalValue = 0;
                for (var i = start; i < border; i++)
                {
                    totalValue += sidePots[i].Value;
                }
            
                mergedSidePots.Add(new Pot(totalValue, sidePots[start].Contenders));
                start = border;
            }
            
            return mergedSidePots;
        }
    }
}