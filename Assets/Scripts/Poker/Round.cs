﻿using System;
using System.Collections.Generic;
using System.Linq;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker {
    
    /// <summary>Holds data of a single poker round.</summary>
    public class Round {
        
        private const int MaxCommunityCardCount = 5;

        public event EventHandler<RoundPhaseChangedEventArgs> RoundPhaseChanged;
        public event EventHandler<CurrentPlayerChangedEventArgs> CurrentPlayerChanged;
        
        public int CurrentPot { get; private set; }
        public int CurrentHighestBet => _playerData.Select(player => player.CurrentPhaseBet).Max();

        public Phase CurrentPhase { get; private set; }
        public List<Card> CommunityCards { get; } = new List<Card>();

        /// <summary>The index of the seat whose turn it is at the moment.</summary>
        private int _currentPlayerIndex;
        private int _currentPlayerCount;
        private readonly PlayerData[] _playerData;

        /// <summary>Used to determine if all of the active players have performed the same bet.</summary>
        private int _counter;

        /// <summary>Constructs a new poker round.</summary>
        public Round(TablePlayer[] players, int currentPlayerIndex) {
            if(players == null) throw new ArgumentNullException(nameof(players));
            
            _currentPlayerIndex = currentPlayerIndex;
            _playerData = new PlayerData[players.Length];
            
            for (var i = 0; i < players.Length; i++) {
                if (players[i] != null) {
                    _currentPlayerCount++;
                    _playerData[i] = new PlayerData(players[i]);
                }
            }
        }

        public void Start() {
            UpdateCurrentPlayerIndex();
            OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(Phase.PreFlop));
        }

        /// <summary>Adds a new community card to this round.</summary>
        public void AddCommunityCard(Card card) {
            if (CommunityCards.Count == MaxCommunityCardCount) {
                throw new IndexOutOfRangeException("Maximum number of community cards has already been reached.");
            }
            
            CommunityCards.Add(card);
        }

        /// <summary> Returns all of the players that are currently still playing this round. </summary>
        public List<TablePlayer> GetActivePlayers() {
            return _playerData.Where(data => data != null && !data.Folded).Select(data => data.Player).ToList();
        }

        /// <summary> Returns all of the players that were or still are playing this round. </summary>
        public List<TablePlayer> GetParticipatingPlayers() {
            return _playerData.Where(data => data != null).Select(data => data.Player).ToList();
        }

        //----------------------------------------------------------------
        //                      Player actions
        //----------------------------------------------------------------

        public void PlayerChecked() {
            _counter++;
            UpdateCurrentPlayerIndex();
            CheckForPhaseChange();
        }

        public void PlayerCalled(int callAmount) {
            _counter++;
            ChipsPlaced(_currentPlayerIndex, callAmount);
            UpdateCurrentPlayerIndex();
            CheckForPhaseChange();
        }
        
        public void PlayerFolded() {
            _playerData[_currentPlayerIndex].Folded = true;
            _currentPlayerCount--;

            if (_currentPlayerCount == 1) {
                OnRoundPhaseChanged(new RoundPhaseChangedEventArgs(Phase.OnePlayerLeft));
            }
            else {
                UpdateCurrentPlayerIndex();
                CheckForPhaseChange();
            }
        }

        public void PlayerRaised(int raiseAmount) {
            _counter = 1;
            ChipsPlaced(_currentPlayerIndex, raiseAmount);
            UpdateCurrentPlayerIndex();
        }
        
        public void PlayerAllIn(int allInAmount) {
            PlayerRaised(allInAmount);
        }
        
        public void ChipsPlaced(int index, int amount) {
            _playerData[index].Player.Stack -= amount;
            _playerData[index].Player.ChipCount -= amount;
            _playerData[index].CurrentPhaseBet += amount;
            _playerData[index].CurrentBet += amount;
            CurrentPot += amount;
        }

        private void UpdateCurrentPlayerIndex() {
            for (int i = 0; i < _playerData.Length - 1; i++) {
                _currentPlayerIndex++;
                _currentPlayerIndex %= _playerData.Length;

                if (_playerData[_currentPlayerIndex] != null) break;
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

        private void OnRoundPhaseChanged(RoundPhaseChangedEventArgs args) {
            if (args.CurrentPhase != Phase.PreFlop) {
                foreach (PlayerData data in _playerData) {
                    data.CurrentPhaseBet = 0;
                }
            }
            
            RoundPhaseChanged?.Invoke(this, args);
        }

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
            Showdown,
            
            /// <summary>Phase in which the round ends because there is only a single player left.</summary>
            OnePlayerLeft
        }

        private class PlayerData {
            public TablePlayer Player { get; }
            public int CurrentBet { get; set; }
            public int CurrentPhaseBet { get; set; }
            public bool Folded { get; set; }

            public PlayerData(TablePlayer player) {
                Player = player;
            }
        }
    }
}