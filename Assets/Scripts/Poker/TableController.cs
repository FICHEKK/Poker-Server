using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dao;
using Poker.Cards;
using Poker.Players;
using RequestProcessors;
using Timer = System.Timers.Timer;

namespace Poker
{
    /// <summary>
    /// Controls and manages a single table. Also handles client's requests and
    /// updates table's state accordingly.
    /// </summary>
    public abstract partial class TableController
    {
        /// <summary>A blocking queue used to store unprocessed client requests.</summary>
        public BlockingCollection<IClientRequestProcessor> RequestProcessors { get; } = new BlockingCollection<IClientRequestProcessor>();

        /// <summary>A dummy action that is used to indicate the end of the consuming.</summary>
        private readonly IClientRequestProcessor _poisonPill = new PoisonPill();

        /// <summary>Table managed by this controller.</summary>
        protected readonly Table Table;

        /// <summary>Table's title (name).</summary>
        public string Title { get; }

        /// <summary>Table's small blind.</summary>
        public int SmallBlind { get; }

        /// <summary>Indicates whether the table is ranked.</summary>
        public abstract bool IsRanked { get; }

        /// <summary> Indicates whether the table cannot be joined. </summary>
        public bool IsLocked { get; protected set; }

        /// <summary>Table's current number of players.</summary>
        public int PlayerCount => Table.PlayerCount;

        /// <summary>Table's maximum number of players.</summary>
        public int MaxPlayers => Table.MaxPlayers;
        
        /// <summary>The current state of the round (table).</summary>
        private Round _round;
        
        /// <summary>The deck used by this controller to deal cards.</summary>
        private readonly Deck _deck = new Deck();
        
        /// <summary> Timer used to time player's turn durations. </summary>
        private readonly Timer _decisionTimer = new Timer {AutoReset = false};

        protected TableController(Table table, string title, int smallBlind)
        {
            Table = table;
            Title = title;
            SmallBlind = smallBlind;

            InitializeTimer();
            new Thread(ConsumeClientRequests).Start();
        }
        
        private void InitializeTimer()
        {
            _decisionTimer.Interval = (TableConstant.PlayerTurnDuration + TableConstant.PlayerTurnOvertime) * 1000;
            _decisionTimer.Elapsed += (sender, e) =>
            {
                if (_round == null) return;
                if (_round.CurrentPhase == Round.Phase.OnePlayerLeft) return;
                if (_round.CurrentPhase == Round.Phase.Showdown) return;
                
                PlayerFold();
            };
        }
        
        private void ConsumeClientRequests()
        {
            while (true)
            {
                var processor = RequestProcessors.Take();
                if (processor == _poisonPill) return;
                processor.ProcessRequest();
            }
        }

        protected void StartNewRound()
        {
            _deck.Shuffle();
            Table.IncrementButtonIndex();

            var smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.DealerButtonIndex);
            var bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);
            var playerIndex = Table.GetNextOccupiedSeatIndex(bigBlindIndex);

            _round = new Round(Table.GetPlayerArray(), smallBlindIndex, bigBlindIndex, playerIndex, SmallBlind);
            _round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            _round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            
            BroadcastBlindsData(smallBlindIndex, bigBlindIndex);
            DealHandCards();
            _round.Start();
        }

        private void ProcessOnePlayerLeft()
        {
            var winner = _round.ActivePlayers[0];
            winner.Stack += _round.Pot;
            winner.ChipCount += _round.Pot;
            
            SendBroadcastPackage(ServerResponse.Showdown, 1, _round.Pot, 1, winner.Index, string.Empty);
            FinishRound(TableConstant.RoundFinishPauseDuration + 1000, new [] {winner});
        }
        
        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(_round.ParticipatingPlayers);
            var winningPlayers = new HashSet<TablePlayer>();

            var package = new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.Showdown)
                .Append(sidePots.Count);

            for (var i = sidePots.Count - 1; i >= 0; i--)
            {
                var sidePotWinners = DetermineWinners(sidePots[i].Contenders, _round.CommunityCards, out var bestHand);
                var winAmount = sidePots[i].Value / sidePotWinners.Count;
                
                foreach (var player in sidePotWinners)
                {
                    player.Stack += winAmount;
                    player.ChipCount += winAmount;
                    winningPlayers.Add(player);
                }
                
                package.Append(sidePots[i].Value)
                       .Append(sidePotWinners.Count)
                       .Append(sidePotWinners, winner => winner.Index)
                       .Append(bestHand.ToStringPretty());
            }
            
            package.Send();
            
            FinishRound(TableConstant.RoundFinishPauseDuration + sidePots.Count * 1000, winningPlayers);
        }
        
        private void FinishRound(int timeout, IEnumerable<TablePlayer> winningPlayers)
        {
            IncrementWinningPlayersWinCount(winningPlayers);
            UpdateParticipatingPlayersChipCount(_round.ParticipatingPlayers);
            Thread.Sleep(timeout);
            KickBrokePlayers();
            OnRoundFinished();
            
            SendBroadcastPackage(ServerResponse.RoundFinished);
        }

        private static void IncrementWinningPlayersWinCount(IEnumerable<TablePlayer> winners)
        {
            foreach (var winner in winners)
                DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
        }

        private static void UpdateParticipatingPlayersChipCount(IEnumerable<TablePlayer> participants)
        {
            foreach (var participant in participants)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
        }
        
        private void KickBrokePlayers()
        {
            foreach (var player in Table)
            {
                if (player.Stack > 0) continue;
                Kick(player);
            }
        }

        //===========================================================
        //                      Template methods
        //===========================================================

        protected abstract void Kick(TablePlayer player);
        protected abstract void OnPlayerJoined();
        protected abstract void OnRoundFinished();
    }
}