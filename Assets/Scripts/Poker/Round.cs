using System;
using System.Collections.Generic;
using System.Linq;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker
{
    /// <summary>Holds data of a single poker round.</summary>
    public class Round
    {
        /// <summary>Maximum number of community cards that can be in the round.</summary>
        private const int MaxCommunityCardCount = 5;

        /// <summary> Value of the pot. </summary>
        public int Pot { get; private set; }

        /// <summary> Index of the currently focused player. </summary>
        public int PlayerIndex { get; private set; }
        
        /// <summary>
        /// List of all the indexes of players that need to pay for big blind since they just joined.
        /// </summary>
        public List<int> JustJoinedPlayerIndexes { get; } = new List<int>();
        
        /// <summary> List of all the current community cards.</summary>
        public List<Card> CommunityCards { get; } = new List<Card>();
        
        /// <summary> List of players that are still playing this round. </summary>
        public List<TablePlayer> ActivePlayers => _players.Where(player => player != null && !player.Folded).ToList();
        
        /// <summary> List of players that were or still are playing this round. </summary>
        public List<TablePlayer> ParticipatingPlayers => _players.Where(player => player != null).ToList();
        
        /// <summary> Highest bet value of the current round phase. </summary>
        private int HighestBet => _players.Where(player => player != null).Select(player => player.Bet).Max();

        private readonly int _smallBlind;
        private int _playerCount;
        private Phase _phase;
        private readonly TablePlayer[] _players;
        private int _betCounter;
        private readonly int _smallBlindIndex;

        /// <summary> Value indicating the current minimum raise that must be made by the raising player. </summary>
        private int _raiseIncrement;

        public event EventHandler<RoundPhaseChangedEventArgs> RoundPhaseChanged;
        public event EventHandler<CurrentPlayerChangedEventArgs> CurrentPlayerChanged;

        /// <summary>Constructs a new poker round.</summary>
        public Round(TablePlayer[] players, int smallBlindIndex, int bigBlindIndex, int playerIndex, int smallBlind)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _smallBlind = smallBlind;
            _raiseIncrement = smallBlind * 2;
            
            _smallBlindIndex = smallBlindIndex;
            PlayerIndex = playerIndex;

            foreach (var player in players)
            {
                if (player == null) continue;
                
                player.Bet = 0;
                player.TotalBet = 0;
                player.Folded = false;
                _playerCount++;

                if (player.HasJustJoined)
                {
                    JustJoinedPlayerIndexes.Add(player.Index);
                    PlaceChips(player.Index, _smallBlind * 2);
                    player.HasJustJoined = false;
                }
            }
            
            if(_players[_smallBlindIndex].Bet == 0)
                PlaceChips(_smallBlindIndex, _smallBlind);
            
            if(_players[bigBlindIndex].Bet == 0)
                PlaceChips(bigBlindIndex, _smallBlind * 2);
        }

        public void Start()
        {
            int requiredCall = HighestBet - _players[PlayerIndex].Bet;
            int minRaise = _players[PlayerIndex].Bet + requiredCall + _raiseIncrement;
            int maxRaise = _players[PlayerIndex].Stack + _players[PlayerIndex].Bet;
            OnCurrentPlayerChanged(new CurrentPlayerChangedEventArgs(PlayerIndex, requiredCall, minRaise, maxRaise));
        }

        /// <summary>Adds a new community card to this round.</summary>
        public void AddCommunityCard(Card card)
        {
            if (CommunityCards.Count == MaxCommunityCardCount)
                throw new IndexOutOfRangeException("Maximum number of community cards has already been reached.");

            CommunityCards.Add(card);
        }

        //----------------------------------------------------------------
        //                      Player actions
        //----------------------------------------------------------------

        public void PlayerChecked()
        {
            _betCounter++;
            UpdateCurrentPlayerIndex();
        }

        public void PlayerCalled(int callAmount)
        {
            _betCounter++;
            PlaceChips(PlayerIndex, callAmount);
            UpdateCurrentPlayerIndex();
        }

        public void PlayerFolded()
        {
            _players[PlayerIndex].Folded = true;
            _playerCount--;

            if (_playerCount == 1)
            {
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(Phase.OnePlayerLeft));
            }
            else
            {
                UpdateCurrentPlayerIndex();
            }
        }

        public void PlayerRaised(int raisedToAmount)
        {
            _betCounter = 1;
            _raiseIncrement = raisedToAmount - HighestBet;
            PlaceChips(PlayerIndex, raisedToAmount - _players[PlayerIndex].Bet);
            UpdateCurrentPlayerIndex();
        }

        public void PlayerAllIn(int allInAmount)
        {
            PlayerRaised(allInAmount);
        }

        public void PlayerLeft(int index)
        {
            if (_players[index] == null || _players[index].Folded) return;
            
            _players[index].Folded = true;
            _playerCount--;

            if (_playerCount == 0) return;

            if (_playerCount == 1)
            {
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(Phase.OnePlayerLeft));
            }
            else if(index == PlayerIndex)
            {
                UpdateCurrentPlayerIndex();
            }
        }

        private void PlaceChips(int index, int amount)
        {
            if (_players[index] == null)
                throw new NullReferenceException("Cannot place chips for an empty seat.");
            
            _players[index].Stack -= amount;
            _players[index].ChipCount -= amount;
            _players[index].Bet += amount;
            _players[index].TotalBet += amount;
        }

        private void UpdateCurrentPlayerIndex()
        {
            bool isPhaseOver = _betCounter == _playerCount;
            PlayerIndex = isPhaseOver ? _smallBlindIndex : PlayerIndex + 1;

            for (int i = 0; i < _players.Length - 1; i++)
            {
                PlayerIndex %= _players.Length;
                if (_players[PlayerIndex] != null && !_players[PlayerIndex].Folded) break;
                PlayerIndex++;
            }
            
            if (isPhaseOver)
            {
                _betCounter = 0;
                _raiseIncrement = _smallBlind * 2;
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(++_phase));

                if (_phase != Phase.Showdown)
                {
                    int requiredCall = 0;
                    int minRaise = _raiseIncrement;
                    int maxRaise = _players[PlayerIndex].Stack;
                    OnCurrentPlayerChanged(new CurrentPlayerChangedEventArgs(PlayerIndex, requiredCall, minRaise, maxRaise));
                }
            }
            else
            {
                int requiredCall = HighestBet - _players[PlayerIndex].Bet;
                int minRaise = HighestBet + _raiseIncrement;
                int maxRaise = _players[PlayerIndex].Stack + _players[PlayerIndex].Bet;
                OnCurrentPlayerChanged(new CurrentPlayerChangedEventArgs(PlayerIndex, requiredCall, minRaise, maxRaise));
            }
        }

        //----------------------------------------------------------------
        //                        Round events
        //----------------------------------------------------------------

        private void OnRoundPhaseChanged(RoundPhaseChangedEventArgs args)
        {
            foreach (var player in _players)
            {
                if (player != null)
                {
                    Pot += player.Bet;
                    player.Bet = 0;
                }
            }

            RoundPhaseChanged?.Invoke(this, args);
        }

        private void OnCurrentPlayerChanged(CurrentPlayerChangedEventArgs args)
        {
            CurrentPlayerChanged?.Invoke(this, args);
        }

        //----------------------------------------------------------------
        //                        Round phases
        //----------------------------------------------------------------
        
        /// <summary>Models table round phases.</summary>
        public enum Phase
        {
            /// <summary>Betting round before the flop cards were revealed.</summary>
            PreFlop,
            
            /// <summary>Betting round after the flop cards were revealed.</summary>
            Flop,

            /// <summary>Betting round after the turn card was revealed.</summary>
            Turn,

            /// <summary>Betting round after the river card was revealed.</summary>
            River,

            /// <summary>Final phase where players show their cards.</summary>
            Showdown,

            /// <summary>Phase in which the round ends because there is only a single player left.</summary>
            OnePlayerLeft
        }
    }
}