using Poker.Players;

namespace Poker
{
    public class StandardTableController : TableController
    {
        public override bool IsRanked => false;
        
        public StandardTableController(Table table, string title, int smallBlind) : base(table, title, smallBlind) { }

        protected override void Kick(TablePlayer player)
        {
            RemovePlayerFromTable(player, ServerResponse.LeaveTableNoMoney);
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
        
        public override void PlayerLeave(TablePlayer player)
        {
            RemovePlayerFromTable(player, ServerResponse.LeaveTableGranted);
            Enqueue(() => Round?.PlayerLeft(player.Index));
        }
    }
}