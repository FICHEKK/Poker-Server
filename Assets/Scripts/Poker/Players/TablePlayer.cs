using System;
using System.IO;
using Poker.Cards;

namespace Poker.Players {
    
    /// <summary> Models a player that is currently on a table. </summary>
    public class TablePlayer : Player {
        
        /// <summary> Table position index of this player. </summary>
        public int Index { get; }

        /// <summary> The seat that this player is currently on. </summary>
        public Table Table { get; set; }

        /// <summary> The amount of chips that this player currently has at the table. </summary>
        public int Stack { get; set; }

        /// <summary> Cards that this player is currently holding. </summary>
        private readonly Card[] _handCards = new Card[2];

        public TablePlayer(string username, int chipCount, Table table, int buyIn, int index, StreamReader reader,
            StreamWriter writer) : base(username, chipCount, reader, writer) {
            Table = table;
            Index = index;
            Stack = buyIn;
        }

        #region Hand cards

        /// <summary> Sets this player's hand cards. </summary>
        /// <param name="card1"> The first hand card. </param>
        /// <param name="card2"> The second hand card. </param>
        public void SetHand(Card card1, Card card2) {
            _handCards[0] = card1 ?? throw new ArgumentNullException(nameof(card1));
            _handCards[1] = card2 ?? throw new ArgumentNullException(nameof(card2));
        }

        /// <summary> Returns this player's first hand card. </summary>
        /// <returns> The first hand card if it exists. If the player currently holds no cards, null is returned. </returns>
        public Card GetFirstHandCard() {
            return _handCards[0];
        }

        /// <summary> Returns this player's first hand card. </summary>
        /// <returns> The second hand card if it exists. If the player currently holds no cards, null is returned. </returns>
        public Card GetSecondHandCard() {
            return _handCards[1];
        }

        #endregion
    }
}