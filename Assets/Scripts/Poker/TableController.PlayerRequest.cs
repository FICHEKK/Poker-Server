using Poker.Players;

namespace Poker
{
    public abstract partial class TableController
    {
        public void PlayerJoin(Client client, int stack)
        {
            SendTableState(client);

            var lobbyPlayer = Casino.GetLobbyPlayer(client.Username);
            Casino.RemoveLobbyPlayer(lobbyPlayer);
            
            var tablePlayer = new TablePlayer(client, lobbyPlayer.ChipCount, this, stack);
            Casino.AddTablePlayer(tablePlayer);
            Table.AddPlayer(tablePlayer);
            
            SendBroadcastPackage(ServerResponse.PlayerJoined, tablePlayer.Index, tablePlayer.Username, tablePlayer.Stack);
            OnPlayerJoined();
        }
        
        private void SendTableState(Client client)
        {
            var package = new Client.Package(client)
                .Append(ServerResponse.TableState)
                .Append(Table.DealerButtonIndex)
                .Append(SmallBlind)
                .Append(Table.MaxPlayers);
            
            AppendPlayerList(package);

            if (_round == null)
            {
                package.Append(0); // community card count
                package.Append(-1); // player index
                package.Append(0); // pot
            }
            else
            {
                package.Append(_round.CommunityCards.Count);
                package.Append(_round.CommunityCards, card => card);
                package.Append(_round.CurrentPlayerIndex);
                package.Append(_round.Pot);
            }
            
            package.Send();
        }

        private void AppendPlayerList(Client.Package package)
        {
            package.Append(Table.PlayerCount);

            foreach (var player in Table)
            {
                package.Append(player.Index)
                       .Append(player.Username)
                       .Append(player.Stack)
                       .Append(player.Bet)
                       .Append(player.Folded);
            }
        }

        // TODO override this in ranked controller
        public virtual void PlayerLeave(TablePlayer player)
        {
            new Client.Package(player.Client)
                .Append(ServerResponse.LeaveTable)
                .Append(ServerResponse.LeaveTableGranted)
                .Send();
            
            Casino.RemoveTablePlayer(player);
            Table.RemovePlayer(player);
            
            Casino.AddLobbyPlayer(new LobbyPlayer(player.Client, player.ChipCount));
            
            SendBroadcastPackage(ServerResponse.PlayerLeft, player.Index);
            


            _round?.PlayerLeft(player.Index);

            if (Table.PlayerCount == 1)
            {
                _round = null;
            }
        }

        public void PlayerCheck()
        {
            SendBroadcastPackage(ServerResponse.PlayerChecked, _round.CurrentPlayerIndex);
            _round.PlayerChecked();
        }

        public void PlayerCall(int callAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerCalled, _round.CurrentPlayerIndex, callAmount);
            _round.PlayerCalled(callAmount);
        }

        public void PlayerFold()
        {
            SendBroadcastPackage(ServerResponse.PlayerFolded, _round.CurrentPlayerIndex);
            _round.PlayerFolded();
        }

        public void PlayerRaise(int raisedToAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerRaised, _round.CurrentPlayerIndex, raisedToAmount);
            _round.PlayerRaised(raisedToAmount);
        }

        public void PlayerAllIn(int allInAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerAllIn, _round.CurrentPlayerIndex, allInAmount);
            _round.PlayerAllIn(allInAmount);
        }

        public void PlayerSendChatMessage(int index, string message)
        {
            SendBroadcastPackage(ServerResponse.ChatMessage, index, message);
        }
    }
}