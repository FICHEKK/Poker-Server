using System;
using System.Collections.Generic;
using System.Threading;
using Dao;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker
{
    /// <summary>Models a poker table dealer.</summary>
    public class Dealer
    {
        /// <summary>The table that this dealer is dealing on.</summary>
        public Table Table { get; }

        /// <summary>The current state of the round.</summary>
        public Round Round { get; private set; }

        /// <summary>The deck used by this dealer to deal cards.</summary>
        public Deck Deck { get; } = new Deck();

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

            int smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.ButtonIndex);
            int bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);

            Broadcast(ServerResponse.Blinds);
            Broadcast(smallBlindIndex.ToString());
            Broadcast(bigBlindIndex.ToString());

            Round = new Round(Table.GetPlayerArray(), bigBlindIndex);
            Round.ChipsPlaced(smallBlindIndex, Table.SmallBlind);
            Round.ChipsPlaced(bigBlindIndex, Table.BigBlind);
            Round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            Round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            Round.Start();
        }

        #region Event handlers

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Player.Index.ToString());
            Broadcast(e.Player.Username);
            Broadcast(e.Player.Stack.ToString());

            if (Table.PlayerCount == 2)
            {
                //StartNewRound();
            }
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e)
        {
            BroadcastToEveryoneExcept(e.Index, ServerResponse.PlayerLeft);
            BroadcastToEveryoneExcept(e.Index, e.Index.ToString());

            // TODO remove player from round and if there is only 1 player left, he wins the round
        }

        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e)
        {
            switch (e.CurrentPhase)
            {
                case Round.Phase.PreFlop: ProcessPreFlop(); break;
                case Round.Phase.Flop: ProcessFlop(); break;
                case Round.Phase.Turn: ProcessTurn(); break;
                case Round.Phase.River: ProcessRiver(); break;
                case Round.Phase.Showdown: ProcessShowdown(); break;
                case Round.Phase.OnePlayerLeft: ProcessOnePlayerLeft(); break;
            }
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
        }

        private void ProcessFlop()
        {
            Broadcast(ServerResponse.Flop);
            RevealCommunityCard();
            RevealCommunityCard();
            RevealCommunityCard();
        }

        private void ProcessTurn()
        {
            Broadcast(ServerResponse.Turn);
            RevealCommunityCard();
        }

        private void ProcessRiver()
        {
            Broadcast(ServerResponse.River);
            RevealCommunityCard();
        }

        private void RevealCommunityCard()
        {
            Card card = Deck.GetNextCard();
            Broadcast(card.ToString());
            Round.AddCommunityCard(card);
        }

        private void ProcessShowdown()
        {
            FinishThisRound(DetermineWinners());
        }

        private void ProcessOnePlayerLeft()
        {
            FinishThisRound(new List<TablePlayer> {Round.GetActivePlayers()[0]});
        }

        private void FinishThisRound(List<TablePlayer> winners)
        {
            int winAmount = Round.CurrentPot / winners.Count;

            foreach (TablePlayer winner in winners)
            {
                DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
                winner.Stack += winAmount;
                winner.ChipCount += winAmount;
            }

            foreach (TablePlayer participant in Round.GetParticipatingPlayers())
            {
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            }

            Broadcast(ServerResponse.Showdown);
            Broadcast(winners.Count.ToString());

            foreach (TablePlayer winner in winners)
            {
                Broadcast(winner.Index.ToString());
            }

            Thread.Sleep(2000);
            Broadcast(ServerResponse.RoundFinished);
            StartNewRound();
        }

        private List<TablePlayer> DetermineWinners()
        {
            List<Card> cards = Round.CommunityCards;
            List<TablePlayer> winners = new List<TablePlayer>();

            Hand bestHand = null;

            foreach (TablePlayer player in Round.GetActivePlayers())
            {
                Card handCard1 = player.GetFirstHandCard();
                Card handCard2 = player.GetSecondHandCard();

                SevenCardEvaluator evaluator = new SevenCardEvaluator(handCard1, handCard2,
                    cards[0], cards[1], cards[2], cards[3], cards[4]);

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

        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex.ToString());

            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, Round.CurrentHighestBet.ToString());
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

        public void BroadcastToEveryoneExcept(int index, ServerResponse response)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                if (i == index) continue;
                Signal(i, response);
            }
        }

        public void BroadcastToEveryoneExcept(int index, string data)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                if (i == index) continue;
                Signal(i, data);
            }
        }

        #endregion
    }
}