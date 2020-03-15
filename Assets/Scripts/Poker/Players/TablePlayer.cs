using System;
using System.IO;
using Poker.Cards;

namespace Poker.Players
{
    /// <summary> Models a player that is currently on a table. </summary>
    public class TablePlayer : Player
    {
        /// <summary> Table position index of this player. </summary>
        public int Index { get; }

        /// <summary> The seat that this player is currently on. </summary>
        public Table Table { get; }

        /// <summary> The amount of chips that this player currently has at the table. </summary>
        public int Stack { get; set; }
        
        /// <summary> Amount of chips placed by this player in the current betting phase. </summary>
        public int Bet { get; set; }
        
        /// <summary> Total amount of chips placed by this player in the current poker round. </summary>
        public int TotalBet { get; set; }

        /// <summary>
        /// Flag indicating whether this player is currently in 'folded' state.
        /// Defaults to true as a newly joined table player is in waiting (folded) state.
        /// </summary>
        public bool Folded { get; set; } = true;

        /// <summary> This player's first hand card. </summary>
        public Card FirstHandCard => _handCards[0];
        
        /// <summary> This player's second hand card. </summary>
        public Card SecondHandCard => _handCards[1];

        /// <summary> Cards that this player is currently holding. </summary>
        private readonly Card[] _handCards = new Card[2];

        public TablePlayer(string username, int chipCount, Table table, int buyIn, int index, StreamReader reader,
            StreamWriter writer) : base(username, chipCount, reader, writer)
        {
            Table = table;
            Index = index;
            Stack = buyIn;
        }

        /// <summary> Sets this player's hand cards. </summary>
        /// <param name="card1"> The first hand card. </param>
        /// <param name="card2"> The second hand card. </param>
        public void SetHand(Card card1, Card card2)
        {
            _handCards[0] = card1 ?? throw new ArgumentNullException(nameof(card1));
            _handCards[1] = card2 ?? throw new ArgumentNullException(nameof(card2));
        }
    }
}