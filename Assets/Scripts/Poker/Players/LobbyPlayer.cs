namespace Poker.Players
{
    /// <summary> Models a player that is currently in lobby. </summary>
    public class LobbyPlayer : Player
    {
        /// <summary> Constructs a new lobby player. </summary>
        public LobbyPlayer(Client client, int chipCount) : base(client, chipCount) { }
    }
}