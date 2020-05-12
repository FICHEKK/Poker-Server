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
        public int CurrentPlayerIndex { get; private set; }
        
        /// <summary> List of all the indexes of players that need to pay for big blind since they just joined. </summary>
        public List<int> JustJoinedPlayerIndexes { get; } = new List<int>();
        
        /// <summary> List of all the current community cards.</summary>
        public List<Card> CommunityCards { get; } = new List<Card>();
        
        /// <summary> List of players that are still playing this round. </summary>
        public List<TablePlayer> ActivePlayers => _players.Where(player => player != null && !player.Folded).ToList();
        
        /// <summary> List of players that were or still are playing this round. </summary>
        public List<TablePlayer> ParticipatingPlayers => _players.Where(player => player != null).ToList();
        
        /// <summary> Highest bet value of the current round phase. </summary>
        private int HighestBet => _players.Where(player => player != null).Select(player => player.Bet).Max();
        
        /// <summary>The phase that this round is currently in.</summary>
        public Phase CurrentPhase;

        private int _playerCount;
        private int _betCounter;
        private int _raiseIncrement;
        private readonly int _smallBlind;
        private readonly int _smallBlindIndex;
        private readonly TablePlayer[] _players;

        public event EventHandler<RoundPhaseChangedEventArgs> RoundPhaseChanged;
        public event EventHandler<CurrentPlayerChangedEventArgs> CurrentPlayerChanged;
        
        public Round(TablePlayer[] players, int smallBlindIndex, int bigBlindIndex, int currentPlayerIndex, int smallBlind)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _smallBlind = smallBlind;
            _raiseIncrement = smallBlind * 2;
            
            _smallBlindIndex = smallBlindIndex;
            CurrentPlayerIndex = currentPlayerIndex;

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
            SendBettingDataToCurrentPlayer();
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
            PlaceChips(CurrentPlayerIndex, callAmount);
            UpdateCurrentPlayerIndex();
        }

        public void PlayerFolded()
        {
            _players[CurrentPlayerIndex].Folded = true;
            _playerCount--;

            if (_playerCount == 1)
            {
                CurrentPhase = Phase.OnePlayerLeft;
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(CurrentPhase));
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
            PlaceChips(CurrentPlayerIndex, raisedToAmount - _players[CurrentPlayerIndex].Bet);
            UpdateCurrentPlayerIndex();
        }

        public void PlayerAllIn(int allInAmount)
        {
            PlayerRaised(allInAmount);
        }

        public void PlayerLeft(int index)
        {
            if (CurrentPhase == Phase.Showdown) return;
            if (CurrentPhase == Phase.OnePlayerLeft) return;
            if (_players[index] == null || _players[index].Folded) return;
            
            _players[index].Folded = true;
            _playerCount--;

            if (_playerCount == 0) return;

            if (_playerCount == 1)
            {
                CurrentPhase = Phase.OnePlayerLeft;
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(CurrentPhase));
            }
            else if(index == CurrentPlayerIndex)
            {
                UpdateCurrentPlayerIndex();
            }
        }

        private void UpdateCurrentPlayerIndex()
        {
            FindFirstValidIndexFrom(CurrentPlayerIndex + 1);

            if (_betCounter >= _playerCount)
            {
                GoToNextPhase();
            }
            else
            {
                SendBettingDataToCurrentPlayer();
            }
        }

        private void GoToNextPhase()
        {
            _betCounter = 0;
            _raiseIncrement = _smallBlind * 2;
            OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(++CurrentPhase));
            
            // There are at least 2 players who can bet.
            if (_players.Count(p => p?.Stack > 0) >= 2)
            {
                if (CurrentPhase == Phase.Showdown) return;
                
                FindFirstValidIndexFrom(_smallBlindIndex);
                SendBettingDataToCurrentPlayer();
            }
            else
            {
                while (CurrentPhase != Phase.Showdown)
                {
                    OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(++CurrentPhase));
                }
            }
        }

        private void FindFirstValidIndexFrom(int startingIndex)
        {
            CurrentPlayerIndex = startingIndex;
            
            for (int i = 0; i < _players.Length - 1; i++)
            {
                CurrentPlayerIndex %= _players.Length;
            
                if (_players[CurrentPlayerIndex] != null && !_players[CurrentPlayerIndex].Folded)
                {
                    if (_players[CurrentPlayerIndex].Stack == 0)
                    {
                        if(++_betCounter >= _playerCount) break;
                    }
                    else
                    {
                        break;
                    }
                }
            
                CurrentPlayerIndex++;
            }
        }

        private void SendBettingDataToCurrentPlayer()
        {
            var requiredCall = Math.Min(HighestBet - _players[CurrentPlayerIndex].Bet, _players[CurrentPlayerIndex].Stack);
            var minRaise = HighestBet + _raiseIncrement;
            var maxRaise = _players[CurrentPlayerIndex].Stack + _players[CurrentPlayerIndex].Bet;
            CurrentPlayerChanged?.Invoke(this, new CurrentPlayerChangedEventArgs(CurrentPlayerIndex, requiredCall, minRaise, maxRaise));
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
        
        public void AddCommunityCard(Card card)
        {
            if (CommunityCards.Count == MaxCommunityCardCount)
                throw new IndexOutOfRangeException("Maximum number of community cards has already been reached.");

            CommunityCards.Add(card);
        }

        //----------------------------------------------------------------
        //                        Round events
        //----------------------------------------------------------------

        private void OnRoundPhaseChanged(RoundPhaseChangedEventArgs args)
        {
            CollectBets();
            RoundPhaseChanged?.Invoke(this, args);
        }

        private void CollectBets()
        {
            foreach (var player in _players)
            {
                if (player == null) continue;
                Pot += player.Bet;
                player.Bet = 0;
            }
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