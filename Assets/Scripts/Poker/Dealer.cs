using System;
using System.Collections.Generic;
using System.Linq;
using Dao;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker
{
    /// <summary>Models a poker table dealer.</summary>
    public class Dealer
    {
        /// <summary>Pause duration in-between 2 rounds (in milliseconds).</summary>
        private const int RoundFinishPauseDuration = 5000;

        /// <summary>Pause duration for a single card to flip over.</summary>
        private const int PausePerCardDuration = 500;

        /// <summary>The table that this dealer is dealing on.</summary>
        public Table Table { get; }

        /// <summary>The current state of the round.</summary>
        public Round Round { get; private set; }

        /// <summary>The deck used by this dealer to deal cards.</summary>
        public Deck Deck { get; } = new Deck();

        /// <summary> True if there are not enough players for the round to start. </summary>
        public bool IsWaitingForPlayers => Round == null;

        public Dealer(Table table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Table.PlayerJoined += PlayerJoinedEventHandler;
            Table.PlayerLeft += PlayerLeftEventHandler;
        }

        private void StartNewRound()
        {
            Deck.Shuffle();
            Table.IncrementButtonIndex();

            int smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.DealerButtonIndex);
            int bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);
            int playerIndex = Table.GetNextOccupiedSeatIndex(bigBlindIndex);

            Round = new Round(Table.GetPlayerArray(), smallBlindIndex, bigBlindIndex, playerIndex, Table.SmallBlind);
            Round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            Round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            
            BroadcastBlindsData(smallBlindIndex, bigBlindIndex);
            ProcessPreFlop();
            Round.Start();
        }

        private void BroadcastBlindsData(int smallBlindIndex, int bigBlindIndex)
        {
            Broadcast(ServerResponse.Blinds);
            
            Broadcast(Round.JustJoinedPlayerIndexes.Count.ToString());
            foreach (var index in Round.JustJoinedPlayerIndexes)
                Broadcast(index.ToString());

            Broadcast(Table.DealerButtonIndex.ToString());
            Broadcast(smallBlindIndex.ToString());
            Broadcast(bigBlindIndex.ToString());
        }

        private void ProcessPreFlop()
        {
            Broadcast(ServerResponse.Hand);

            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                if (!Table.IsSeatOccupied(i)) continue;

                Card handCard1 = Deck.GetNextCard();
                Card handCard2 = Deck.GetNextCard();

                Table.GetPlayerAt(i).SetHand(handCard1, handCard2);

                Signal(i, handCard1.ToString());
                Signal(i, handCard2.ToString());
            }
            
            Wait(PausePerCardDuration * 2);
        }

        private void ProcessFlop()
        {
            Broadcast(ServerResponse.Flop);
            RevealCommunityCard();
            RevealCommunityCard();
            RevealCommunityCard();
            Wait(PausePerCardDuration * 3);
        }

        private void ProcessTurn()
        {
            Broadcast(ServerResponse.Turn);
            RevealCommunityCard();
            Wait(PausePerCardDuration);
        }

        private void ProcessRiver()
        {
            Broadcast(ServerResponse.River);
            RevealCommunityCard();
            Wait(PausePerCardDuration);
        }

        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(Round.ParticipatingPlayers);
            var alreadyIncreasedWinCountPlayers = new HashSet<TablePlayer>();
            
            // [sidePotCount] ([sidePotValue] [winnerCount] [winnerIndexes]*)*
            Broadcast(ServerResponse.Showdown);
            Broadcast(sidePots.Count.ToString());

            for (int i = sidePots.Count - 1; i >= 0; i--)
            {
                var winners = DetermineWinners(sidePots[i].Contenders, Round.CommunityCards);
                var winAmount = sidePots[i].Value / winners.Count;
                
                foreach (var player in winners)
                {
                    player.Stack += winAmount;
                    player.ChipCount += winAmount;

                    if (alreadyIncreasedWinCountPlayers.Contains(player)) continue;
                    
                    DaoProvider.Dao.SetWinCount(player.Username, DaoProvider.Dao.GetWinCount(player.Username) + 1);
                    alreadyIncreasedWinCountPlayers.Add(player);
                }
                
                Broadcast(sidePots[i].Value.ToString());
                Broadcast(winners.Count.ToString());
                foreach (var player in winners)
                    Broadcast(player.Index.ToString());
            }

            foreach (var participant in Round.ParticipatingPlayers)
            {
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            }

            Wait(RoundFinishPauseDuration + sidePots.Count * 1000);
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount > 1)
            {
                StartNewRound();
            }
        }
        
        private void RevealActivePlayersCards()
        {
            Broadcast(ServerResponse.CardsReveal);
            Broadcast(Round.ActivePlayers.Count.ToString());
            
            foreach (var player in Round.ActivePlayers)
            {
                Broadcast(player.Index.ToString());
                Broadcast(player.FirstHandCard.ToString());
                Broadcast(player.SecondHandCard.ToString());
            }
        }

        private static List<TablePlayer> DetermineWinners(List<TablePlayer> players, List<Card> communityCards)
        {
            var winners = new List<TablePlayer>();

            Hand bestHand = null;

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

                int result = bestHand.CompareTo(evaluator.BestHand);

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

        private void ProcessOnePlayerLeft()
        {
            var winner = Round.ActivePlayers[0];
            
            Broadcast(ServerResponse.Showdown);
            Broadcast(1.ToString());
            Broadcast(Round.Pot.ToString());
            Broadcast(1.ToString());
            Broadcast(winner.Index.ToString());
            
            DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
            winner.Stack += Round.Pot;
            winner.ChipCount += Round.Pot;
            
            foreach (var participant in Round.ParticipatingPlayers)
            {
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            }

            Wait(RoundFinishPauseDuration);
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount > 1)
            {
                StartNewRound();
            }
        }
        
        #region Event handlers
        
        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e)
        {
            switch (e.CurrentPhase)
            {
                case Round.Phase.Flop: ProcessFlop(); break;
                case Round.Phase.Turn: ProcessTurn(); break;
                case Round.Phase.River: ProcessRiver(); break;
                case Round.Phase.Showdown: ProcessShowdown(); break;
                case Round.Phase.OnePlayerLeft: ProcessOnePlayerLeft(); break;
            }
        }

        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex.ToString());
            
            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, e.RequiredCall.ToString());
            Signal(e.CurrentPlayerIndex, e.MinRaise.ToString());
            Signal(e.CurrentPlayerIndex, e.MaxRaise.ToString());
        }
        
        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Player.Index.ToString());
            Broadcast(e.Player.Username);
            Broadcast(e.Player.Stack.ToString());

            if (Table.PlayerCount == 2)
            {
                StartNewRound();
            }
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e)
        {
            Broadcast(ServerResponse.PlayerLeft);
            Broadcast(e.Index.ToString());

            Round?.PlayerLeft(e.Index);

            if (Table.PlayerCount == 1)
            {
                Round = null;
            }
        }

        #endregion

        #region Helper methods
        
        private void RevealCommunityCard()
        {
            Card card = Deck.GetNextCard();
            Broadcast(card.ToString());
            Round.AddCommunityCard(card);
        }

        private void Wait(int milliseconds)
        {
            Broadcast(ServerResponse.WaitForMilliseconds);
            Broadcast(milliseconds.ToString());
        }

        #endregion

        #region Signal and broadcast

        /// <summary> Sends a server response to the single specified position. </summary>
        /// <param name="index"> The index of the player to send a response to. </param>
        /// <param name="response"> The response to be sent. </param>
        public void Signal(int index, ServerResponse response)
        {
            Table.GetPlayerAt(index)?.Writer.BaseStream.WriteByte((byte) response);
        }

        /// <summary> Sends given data to the single specified position. </summary>
        /// <param name="index"> The index of the client to send a response to. </param>
        /// <param name="data"> The data to be sent. </param>
        public void Signal(int index, string data)
        {
            Table.GetPlayerAt(index)?.Writer.WriteLine(data);
        }

        /// <summary> Sends a server response to every player on the table. </summary>
        /// <param name="response"> The response to be sent. </param>
        public void Broadcast(ServerResponse response)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                Signal(i, response);
            }
        }

        /// <summary> Sends given data to every player on the table. </summary>
        /// <param name="data"> The data to be sent. </param>
        public void Broadcast(string data)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                Signal(i, data);
            }
        }

        #endregion
    }
}