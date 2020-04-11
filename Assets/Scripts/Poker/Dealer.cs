using System;
using System.Collections.Generic;
using System.Timers;
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

        /// <summary> True if there are not enough players for the round to start. </summary>
        public bool IsWaitingForPlayers => Round == null;
        
        /// <summary> Timer used to time player's turn durations. </summary>
        private readonly Timer _decisionTimer = new Timer {AutoReset = false};
        
        /// <summary> A collection of waiting times used by the decision timer. </summary>
        private readonly Stack<int> _waitingTimes = new Stack<int>();

        public Dealer(Table table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Table.PlayerJoined += PlayerJoinedEventHandler;
            Table.PlayerLeft += PlayerLeftEventHandler;
            
            _decisionTimer.Elapsed += (sender, e) =>
            {
                if (IsWaitingForPlayers) return;
                Broadcast(ServerResponse.PlayerFolded);
                Broadcast(Round.PlayerIndex);
                Round.PlayerFolded();
            };
        }
        
        //----------------------------------------------------------------
        //                     Starting new round
        //----------------------------------------------------------------

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
            DealHandCards();
            Round.Start();
        }

        private void BroadcastBlindsData(int smallBlindIndex, int bigBlindIndex)
        {
            Broadcast(ServerResponse.Blinds);
            
            Broadcast(Round.JustJoinedPlayerIndexes.Count);
            foreach (var index in Round.JustJoinedPlayerIndexes) Broadcast(index);

            Broadcast(Table.DealerButtonIndex);
            Broadcast(smallBlindIndex);
            Broadcast(bigBlindIndex);
        }

        private void DealHandCards()
        {
            Broadcast(ServerResponse.Hand);

            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                if (!Table.IsSeatOccupied(i)) continue;

                Card handCard1 = Deck.GetNextCard();
                Card handCard2 = Deck.GetNextCard();

                Table.GetPlayerAt(i).SetHand(handCard1, handCard2);

                Signal(i, handCard1);
                Signal(i, handCard2);
            }
        }
        
        //----------------------------------------------------------------
        //                    Flop, turn & river
        //----------------------------------------------------------------

        private void ProcessPhase(ServerResponse response, int cardCount)
        {
            Broadcast(response);
            for(int i = 0; i < cardCount; i++) RevealCommunityCard();
        }

        private void RevealCommunityCard()
        {
            Card card = Deck.GetNextCard();
            Broadcast(card);
            Round.AddCommunityCard(card);
        }

        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(Round.ParticipatingPlayers);
            var winningPlayers = new HashSet<TablePlayer>();
            
            Broadcast(ServerResponse.Showdown);
            Broadcast(sidePots.Count);

            for (int i = sidePots.Count - 1; i >= 0; i--)
            {
                var sidePotWinners = DetermineWinners(sidePots[i].Contenders, Round.CommunityCards);
                var winAmount = sidePots[i].Value / sidePotWinners.Count;
                
                foreach (var player in sidePotWinners)
                {
                    player.Stack += winAmount;
                    player.ChipCount += winAmount;
                    winningPlayers.Add(player);
                }

                BroadcastSidePotData(sidePots[i].Value, sidePotWinners.Count, sidePotWinners);
            }
            
            foreach (var player in winningPlayers)
                DaoProvider.Dao.SetWinCount(player.Username, DaoProvider.Dao.GetWinCount(player.Username) + 1);

            foreach (var participant in Round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            
            Wait(TableConstant.RoundFinishPauseDuration + sidePots.Count * 1000);
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount >= 2) StartNewRound();
        }

        private void BroadcastSidePotData(int value, int winnerCount, List<TablePlayer> winners)
        {
            Broadcast(value);
            Broadcast(winnerCount);
            foreach (var winner in winners) Broadcast(winner.Index);
        }

        private void RevealActivePlayersCards()
        {
            Broadcast(ServerResponse.CardsReveal);
            Broadcast(Round.ActivePlayers.Count);
            
            foreach (var player in Round.ActivePlayers)
            {
                Broadcast(player.Index);
                Broadcast(player.FirstHandCard);
                Broadcast(player.SecondHandCard);
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
            Broadcast(1);
            Broadcast(Round.Pot);
            Broadcast(1);
            Broadcast(winner.Index);
            
            DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
            winner.Stack += Round.Pot;
            winner.ChipCount += Round.Pot;
            
            foreach (var participant in Round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);

            Wait(TableConstant.RoundFinishPauseDuration);
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount >= 2) StartNewRound();
        }

        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e)
        {
            switch (e.CurrentPhase)
            {
                case Round.Phase.Flop: ProcessPhase(ServerResponse.Flop, 3); break;
                case Round.Phase.Turn: ProcessPhase(ServerResponse.Turn, 1); break;
                case Round.Phase.River: ProcessPhase(ServerResponse.River, 1); break;
                case Round.Phase.Showdown: ProcessShowdown(); break;
                case Round.Phase.OnePlayerLeft: ProcessOnePlayerLeft(); break;
            }
        }

        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex);
            
            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, e.RequiredCall);
            Signal(e.CurrentPlayerIndex, e.MinRaise);
            Signal(e.CurrentPlayerIndex, e.MaxRaise);
            
            StartDecisionTimer();
        }

        private void StartDecisionTimer()
        {
            double interval = (TableConstant.PlayerTurnDuration + TableConstant.PlayerTurnOvertime) * 1000;
            
            while (_waitingTimes.Count > 0)
                interval += _waitingTimes.Pop();

            _decisionTimer.Interval = interval;
            _decisionTimer.Stop();
            _decisionTimer.Start();
        }

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Player.Index);
            Broadcast(e.Player.Username);
            Broadcast(e.Player.Stack);

            if (Table.PlayerCount == 2) StartNewRound();
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e)
        {
            Broadcast(ServerResponse.PlayerLeft);
            Broadcast(e.Index);

            Round?.PlayerLeft(e.Index);

            if (Table.PlayerCount == 1)
            {
                Round = null;
            }
        }

        private void Wait(int milliseconds)
        {
            Broadcast(ServerResponse.WaitForMilliseconds);
            Broadcast(milliseconds);
            _waitingTimes.Push(milliseconds);
        }

        /// <summary> Sends a server response to the single specified position. </summary>
        /// <param name="index"> The index of the player to send a response to. </param>
        /// <param name="response"> The response to be sent. </param>
        private void Signal(int index, ServerResponse response) =>
            Table.GetPlayerAt(index)?.Writer.BaseStream.WriteByte((byte) response);

        /// <summary> Sends given data to the single specified position. </summary>
        /// <param name="index"> The index of the client to send a response to. </param>
        /// <param name="data"> The data to be sent. </param>
        private void Signal<T>(int index, T data) =>
            Table.GetPlayerAt(index)?.Writer.WriteLine(data.ToString());

        /// <summary> Sends a server response to every player on the table. </summary>
        /// <param name="response"> The response to be sent. </param>
        public void Broadcast(ServerResponse response)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
                Signal(i, response);
        }

        /// <summary> Sends given data to every player on the table. </summary>
        /// <param name="data"> The data to be sent. </param>
        public void Broadcast<T>(T data)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
                Signal(i, data);
        }
    }
}