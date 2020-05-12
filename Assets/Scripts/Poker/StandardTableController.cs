﻿using Poker.Players;

namespace Poker
{
    public class StandardTableController : TableController
    {
        public override bool IsRanked => false;
        
        public StandardTableController(Table table, string title, int smallBlind) : base(table, title, smallBlind) { }

        protected override void Kick(TablePlayer player)
        {
            new Client.Package(player.Client)
                .Append(ServerResponse.LeaveTable)
                .Append(ServerResponse.LeaveTableNoMoney)
                .Send();

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