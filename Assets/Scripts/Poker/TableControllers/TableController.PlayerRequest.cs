using Poker.Players;

namespace Poker.TableControllers
{
    public abstract partial class TableController
    {
        public abstract void PlayerLeave(TablePlayer player);
        
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

            if (Round == null)
            {
                package.Append(0); // community card count
                package.Append(-1); // player index
                package.Append(0); // pot
            }
            else
            {
                package.Append(Round.CommunityCards.Count);
                package.Append(Round.CommunityCards, card => card);
                package.Append(Round.CurrentPlayerIndex);
                package.Append(Round.Pot);
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


        public void PlayerCheck()
        {
            SendBroadcastPackage(ServerResponse.PlayerChecked, Round.CurrentPlayerIndex);
            Round.PlayerChecked();
        }

        public void PlayerCall(int callAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerCalled, Round.CurrentPlayerIndex, callAmount);
            Round.PlayerCalled(callAmount);
        }

        public void PlayerFold()
        {
            SendBroadcastPackage(ServerResponse.PlayerFolded, Round.CurrentPlayerIndex);
            Round.PlayerFolded();
        }

        public void PlayerRaise(int raisedToAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerRaised, Round.CurrentPlayerIndex, raisedToAmount);
            Round.PlayerRaised(raisedToAmount);
        }

        public void PlayerAllIn(int allInAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerAllIn, Round.CurrentPlayerIndex, allInAmount);
            Round.PlayerAllIn(allInAmount);
        }

        public void PlayerSendChatMessage(int index, string message)
        {
            SendBroadcastPackage(ServerResponse.ChatMessage, index, message);
        }
    }
}