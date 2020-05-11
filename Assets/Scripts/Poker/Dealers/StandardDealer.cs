using Poker.Players;

namespace Poker.Dealers
{
    public class StandardDealer : Dealer
    {
        public StandardDealer(Table table) : base(table) { }

        protected override void Kick(TablePlayer player)
        {
            var package = new Client.Package(player.Client);
            package.Append(ServerResponse.LeaveTable);
            package.Append(ServerResponse.LeaveTableNoMoney);
            package.Send();
            
            Casino.RemoveTablePlayer(player);
            Casino.AddLobbyPlayer(new LobbyPlayer(player.Client, player.ChipCount));
        }
        
        protected override void OnPlayerJoined()
        {
            if (Table.PlayerCount == 2)
                StartNewRound();
        }
        
        protected override void OnRoundFinished()
        {
            if (Table.PlayerCount >= 2) 
                StartNewRound();
        }
    }
}