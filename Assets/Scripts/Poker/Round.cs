using System;
using System.Collections.Generic;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker {
    
    /// <summary>Holds data of a single poker round.</summary>
    public class Round {
        
        private const int MaxCommunityCardCount = 5;
        private const int MinSeatCountAtStart = 2;

        public event EventHandler<RoundPhaseChangedEventArgs> RoundPhaseChanged;
        public event EventHandler<CurrentPlayerChangedEventArgs> CurrentPlayerChanged;
        
        public int CurrentPot { get; private set; }
        public int CurrentHighestBet { get; set; }
        public Phase CurrentPhase { get; private set; }

        private readonly int _smallBlind;
        private readonly TablePlayer[] _players;
        private readonly List<Card> _communityCards = new List<Card>();

        /// <summary>The index of the seat whose turn it is at the moment.</summary>
        private int _currentPlayerIndex;
        private int _currentPlayerCount;

        /// <summary>Used to determine if all of the active players have performed the same bet.</summary>
        private int _counter;

        /// <summary>Constructs a new poker round.</summary>
        public Round(int smallBlind, TablePlayer[] players, int currentPlayerIndex) {
            _players = players ?? throw new ArgumentNullException(nameof(players));

            _smallBlind = smallBlind;
            CurrentHighestBet = _smallBlind * 2;
            _currentPlayerIndex = currentPlayerIndex;

            for (int i = 0; i < _players.Length; i++) {
                if (_players[i] != null) {
                    _currentPlayerCount++;
                }
            }
        }

        public void Start() {
            OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(Phase.PreFlop));
            UpdateCurrentPlayerIndex();
        }

        /// <summary>Adds a new community card to this round.</summary>
        public void AddCommunityCard(Card card) {
            if (_communityCards.Count == MaxCommunityCardCount) {
                throw new IndexOutOfRangeException("Maximum number of community cards has already been reached.");
            }
            
            _communityCards.Add(card);
        }

        /// <summary>Adds the specified amount of chips to the current pot.</summary>
        public void AddToPot(int amount) {
            CurrentPot += amount;
        }

        public void PlayerChecked() {
            _counter++;
            CheckForPhaseChange();
            UpdateCurrentPlayerIndex();
        }

        public void PlayerCalled(int callAmount) {
            _counter++;
            _players[_currentPlayerIndex].CurrentBet = callAmount;
            CheckForPhaseChange();
            UpdateCurrentPlayerIndex();
        }
        
        public void PlayerFolded() {
            _players[_currentPlayerIndex] = null;
            _currentPlayerCount--;
            CheckForPhaseChange();
            UpdateCurrentPlayerIndex();
        }

        public void PlayerRaised(int raiseAmount) {
            _counter = 1;
            _players[_currentPlayerIndex].CurrentBet = raiseAmount;
            UpdateCurrentPlayerIndex();
        }
        
        public void PlayerAllIn(int allInAmount) {
            PlayerRaised(allInAmount);
        }

        private void UpdateCurrentPlayerIndex() {
            for (int i = 0; i < _players.Length - 1; i++) {
                _currentPlayerIndex++;
                _currentPlayerIndex %= _players.Length;

                if (_players[_currentPlayerIndex] != null) break;
            }
            
            OnCurrentPlayerChanged(new CurrentPlayerChangedEventArgs(_currentPlayerIndex));
        }

        /// <summary>
        /// Checks if all of the players have done the same action up to this point.
        /// If so, next round phase should begin.
        /// </summary>
        private void CheckForPhaseChange() {
            if (_counter == _currentPlayerCount) {
                _counter = 0;
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(++CurrentPhase));
            }
        }

        private void OnRoundPhaseChanged(RoundPhaseChangedEventArgs args) => RoundPhaseChanged?.Invoke(this, args);
        private void OnCurrentPlayerChanged(CurrentPlayerChangedEventArgs args) => CurrentPlayerChanged?.Invoke(this, args);
        
        /// <summary>Models table round phases.</summary>
        public enum Phase {

            /// <summary>First betting round where the hand cards are dealt.</summary>
            PreFlop,
        
            /// <summary>Second betting round after the flop cards were revealed.</summary>
            Flop,
        
            /// <summary>Third betting round after the turn card was revealed.</summary>
            Turn,
        
            /// <summary>Fourth betting round after the river card was revealed.</summary>
            River,
            
            /// <summary>Final phase where players show their cards.</summary>
            Showdown
        }
    }
}