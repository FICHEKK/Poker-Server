using System;
using System.Collections.Generic;
using System.Threading;
using Poker.Cards;
using Poker.Players;

namespace Poker
{
    public abstract partial class TableController
    {
        private void SendBroadcastPackage(params object[] items) =>
            new Client.Package(Table.GetActiveClients()).Append(items, item => item).Send();

        private void BroadcastBlindsData(int smallBlindIndex, int bigBlindIndex)
        {
            new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.Blinds)
                .Append(_round.JustJoinedPlayerIndexes.Count)
                .Append(_round.JustJoinedPlayerIndexes, index => index)
                .Append(Table.DealerButtonIndex)
                .Append(smallBlindIndex)
                .Append(bigBlindIndex)
                .Send();
        }
        
        private void DealHandCards()
        {
            foreach (var player in Table)
            {
                var handCard1 = _deck.GetNextCard();
                var handCard2 = _deck.GetNextCard();

                player.SetHand(handCard1, handCard2);

                new Client.Package(player.Client)
                    .Append(ServerResponse.Hand)
                    .Append(handCard1)
                    .Append(handCard2)
                    .Send();
            }
        }
        
        private void RevealCommunityCards(ServerResponse response, int cardCount)
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(response);

            for (var i = 0; i < cardCount; i++)
            {
                var card = _deck.GetNextCard();
                package.Append(card);
                _round.AddCommunityCard(card);
            }
            
            package.Send();
            Thread.Sleep(TableConstant.PausePerCardDuration * cardCount);
        }
        
        private void RevealActivePlayersCards()
        {
            var package = new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.CardsReveal)
                .Append(_round.ActivePlayers.Count);
            
            foreach (var player in _round.ActivePlayers)
            {
                package.Append(player.Index)
                       .Append(player.FirstHandCard)
                       .Append(player.SecondHandCard);
            }
            
            package.Send();
        }
        
        private static List<TablePlayer> DetermineWinners(List<TablePlayer> players, List<Card> communityCards, out Hand bestHand)
        {
            if(players == null) throw new ArgumentNullException(nameof(players));
            if(communityCards == null) throw new ArgumentNullException(nameof(communityCards));
            if(players.Count == 0) throw new ArgumentException("Player collection must be non-empty.");
            if(communityCards.Count != 5) throw new ArgumentException("Expected all 5 community cards.");
            
            var winners = new List<TablePlayer>();

            bestHand = null;

            foreach (var player in players)
            {
                var evaluator = new SevenCardEvaluator(player.FirstHandCard, player.SecondHandCard,
                    communityCards[0], communityCards[1], communityCards[2], communityCards[3], communityCards[4]);

                if (bestHand == null)
                {
                    bestHand = evaluator.BestHand;
                    winners.Add(player);
                    continue;
                }

                var result = bestHand.CompareTo(evaluator.BestHand);

                if (result < 0)
                {
                    winners.Clear();
                    winners.Add(player);
                    bestHand = evaluator.BestHand;
                }
                else if (result == 0)
                {
                    winners.Add(player);
                }
            }

            return winners;
        }
    }
}