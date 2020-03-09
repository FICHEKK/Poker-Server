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
            writer.WriteLine(table.SmallBlind);
            writer.WriteLine(table.PlayerCount);
            writer.WriteLine(table.MaxPlayers);

            for (int index = 0; index < table.MaxPlayers; index++)
            {
                if (table.IsSeatOccupied(index))
                {
                    writer.WriteLine(index);
                    writer.WriteLine(table.GetPlayerAt(index).Username);
                    writer.WriteLine(table.GetPlayerAt(index).Stack);
                }
            }
            
            Casino.MovePlayerFromLobbyToTable(username, table, buyIn);
        }
    }
}