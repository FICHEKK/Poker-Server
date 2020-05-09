using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class JoinTableRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _tableTitle;
        private int _buyIn;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _tableTitle = _client.ReadLine();
            _buyIn = int.Parse(_client.ReadLine());
        }

        public void ProcessRequest()
        {
            var package = new Client.Package(_client);
            
            if (!Casino.HasTableWithTitle(_tableTitle))
            {
                package.Append(ServerResponse.JoinTableTableDoesNotExist);
                package.Send();
                return;
            }

            var table = Casino.GetTable(_tableTitle);

            if (table.PlayerCount == table.MaxPlayers)
            {
                package.Append(ServerResponse.JoinTableTableFull);
                package.Send();
                return;
            }

            package.Append(ServerResponse.JoinTableSuccess);
            package.Send();
            
            // Second response that happens if the joining is successful.
            SendTableState(table);

            LobbyPlayer lobbyPlayer = Casino.GetLobbyPlayer(_client.Username);
            Casino.RemoveLobbyPlayer(lobbyPlayer);
            Casino.AddTablePlayer(new TablePlayer(_client, lobbyPlayer.ChipCount, table, _buyIn, table.GetFirstFreeSeatIndex()));
        }

        private void SendTableState(Table table)
        {
            var package = new Client.Package(_client);
            package.Append(ServerResponse.TableState);
            
            package.Append(table.DealerButtonIndex);
            package.Append(table.SmallBlind);
            package.Append(table.MaxPlayers);
            AppendPlayerList(table, package);

            if (table.Dealer.Round == null)
            {
                package.Append(0); // community card count
                package.Append(-1); // player index
                package.Append(0); // pot
            }
            else
            {
                package.Append(table.Dealer.Round.CommunityCards.Count);
                table.Dealer.Round.CommunityCards.ForEach(card => package.Append(card));
                package.Append(table.Dealer.Round.PlayerIndex);
                package.Append(table.Dealer.Round.Pot);
            }
            
            package.Send();
        }

        private void AppendPlayerList(Table table, Client.Package package)
        {
            package.Append(table.PlayerCount);

            foreach (var player in table)
            {
                package.Append(player.Index);
                package.Append(player.Username);
                package.Append(player.Stack);
                package.Append(player.Bet);
                package.Append(player.Folded);
            }
        }
    }
}