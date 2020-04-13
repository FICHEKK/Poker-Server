using System.IO;
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
            _tableTitle = client.Reader.ReadLine();
            _buyIn = int.Parse(client.Reader.ReadLine());
        }

        public void ProcessRequest()
        {
            if (!Casino.HasTableWithTitle(_tableTitle))
            {
                _client.Writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableDoesNotExist);
                return;
            }

            Table table = Casino.GetTable(_tableTitle);

            if (table.IsFull)
            {
                _client.Writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableFull);
                return;
            }

            _client.Writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.Success);
            _client.Writer.BaseStream.WriteByte((byte) ServerResponse.TableState);
            SendTableState(table, _client.Writer);

            LobbyPlayer lobbyPlayer = Casino.GetLobbyPlayer(_client.Username);
            Casino.RemoveLobbyPlayer(lobbyPlayer);

            int index = table.GetFirstFreeSeatIndex();
            TablePlayer tablePlayer = new TablePlayer(_client.Username, lobbyPlayer.ChipCount, table, _buyIn, index, lobbyPlayer.Reader, lobbyPlayer.Writer);
            Casino.AddTablePlayer(tablePlayer);
        }

        private static void SendTableState(Table table, StreamWriter writer)
        {
            writer.WriteLine(table.DealerButtonIndex);
            writer.WriteLine(table.SmallBlind);
            writer.WriteLine(table.MaxPlayers);
            SendPlayerList(table, writer);

            if (table.Dealer.Round == null)
            {
                writer.WriteLine(0); // community card count
                writer.WriteLine(-1); // player index
                writer.WriteLine(0); // pot
            }
            else
            {
                SendCommunityCardList(table, writer);
                writer.WriteLine(table.Dealer.Round.PlayerIndex);
                writer.WriteLine(table.Dealer.Round.Pot);
            }
        }

        private static void SendPlayerList(Table table, StreamWriter writer)
        {
            writer.WriteLine(table.PlayerCount);
            
            for (int index = 0; index < table.MaxPlayers; index++)
            {
                if (table.IsSeatOccupied(index))
                {
                    writer.WriteLine(index);
                    writer.WriteLine(table.GetPlayerAt(index).Username);
                    writer.WriteLine(table.GetPlayerAt(index).Stack);
                    writer.WriteLine(table.GetPlayerAt(index).Bet);
                    writer.WriteLine(table.GetPlayerAt(index).Folded);
                }
            }
        }
        
        private static void SendCommunityCardList(Table table, StreamWriter writer)
        {
            var communityCards = table.Dealer.Round.CommunityCards;
            writer.WriteLine(communityCards.Count);

            foreach (var card in communityCards)
            {
                writer.WriteLine(card.ToString());
            }
        }
    }
}