using System.IO;
using Poker;

namespace RequestProcessors
{
    public class JoinTableRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string tableTitle = reader.ReadLine();
            int buyIn = int.Parse(reader.ReadLine());

            if (!Casino.HasTableWithTitle(tableTitle))
            {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableDoesNotExist);
                return;
            }

            Table table = Casino.GetTable(tableTitle);

            if (table.IsFull)
            {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableFull);
                return;
            }

            writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.Success);
            SendTableData(table, writer);
            Casino.MovePlayerFromLobbyToTable(username, table, buyIn);
        }

        private static void SendTableData(Table table, StreamWriter writer)
        {
            writer.WriteLine(table.DealerButtonIndex);
            writer.WriteLine(table.SmallBlind);
            writer.WriteLine(table.MaxPlayers);
            SendPlayerList(table, writer);

            if (table.Dealer.IsWaitingForPlayers)
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