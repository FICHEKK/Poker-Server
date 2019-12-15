using Poker.Cards;
using Poker.EventArguments;

namespace Poker {
    public class Dealer {
        private readonly Deck _deck = new Deck();
        private readonly Table _table;

        public Dealer(Table table) {
            _table = table;
            _table.PlayerJoined += PlayerJoinedEventHandler;
            _table.PlayerLeft += PlayerLeftEventHandler;
        }

        public void StartRound() {
            _deck.Shuffle();
        }

        public void DealCards() {
            for (int position = 0; position < _table.MaxPlayers; position++) {
                if(_table.IsSeatEmpty(position)) continue;
                
                _table.Signal(position, ServerResponse.Hand);
                _table.Signal(position, _deck.GetNextCard().ToString());
                _table.Signal(position, _deck.GetNextCard().ToString());
            }
            
            _table.Phase = TablePhase.PreFlop;
        }

        public void RevealFlopCards() {
            _table.Broadcast(ServerResponse.Flop);
            _table.Broadcast(_deck.GetNextCard().ToString());
            _table.Broadcast(_deck.GetNextCard().ToString());
            _table.Broadcast(_deck.GetNextCard().ToString());

            _table.Phase = TablePhase.Flop;
        }

        public void RevealTurnCard() {
            _table.Broadcast(ServerResponse.Turn);
            _table.Broadcast(_deck.GetNextCard().ToString());

            _table.Phase = TablePhase.Turn;
        }

        public void RevealRiverCard() {
            _table.Broadcast(ServerResponse.River);
            _table.Broadcast(_deck.GetNextCard().ToString());

            _table.Phase = TablePhase.River;
        }

        public void FinishRound() {
            _table.IncrementButtonIndex();
            _table.Broadcast(ServerResponse.RoundFinished);
        }

        #region Event handlers

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e) {
            _table.Broadcast(ServerResponse.PlayerJoined);
            _table.Broadcast(e.Username);
            _table.Broadcast(e.ChipCount.ToString());

            if (_table.Phase == TablePhase.Waiting && _table.PlayerCount == 2) {
                StartRound();
                DealCards();
            }
        }
        
        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e) {
            _table.Broadcast(ServerResponse.PlayerLeft);
            _table.Broadcast(e.Index.ToString());
            
            if (_table.PlayerCount == 1) {
                _table.Phase = TablePhase.Waiting;
            }
        }

        #endregion
    }
}
