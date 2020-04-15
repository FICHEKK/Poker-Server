namespace Poker.Players
{
    /// <summary> Abstract implementation of a player. </summary>
    public abstract class Player
    {
        /// <summary> The client connection represented by this player. </summary>
        public Client Client { get; }
        
        /// <summary>This player's username. Shortcut for Client.Username.</summary>
        public string Username => Client.Username;

        /// <summary> The chip count of this player. </summary>
        public int ChipCount { get; set; }

        /// <summary> Constructs a new player. </summary>
        protected Player(Client client, int chipCount)
        {
            Client = client;
            ChipCount = chipCount;
        }

        /// <summary> Players are equal if they have the same username. </summary>
        /// <param name="obj"> Object to compare this player with. </param>
        /// <returns> True if the given object is equal to this player. </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Player player)) return false;
            return Username == player.Username;
        }

        protected bool Equals(Player other)
        {
            return Username == other.Username;
        }

        public override int GetHashCode()
        {
            return Username != null ? Username.GetHashCode() : 0;
        }
    }
}