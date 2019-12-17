using System.IO;
using Poker;

namespace RequestProcessors {
    public class JoinTableRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string tableTitle = reader.ReadLine();
            string username = reader.ReadLine();
            int buyIn = int.Parse(reader.ReadLine());

            if (!Casino.HasTableWithTitle(tableTitle)) {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableDoesNotExist);
                return;
            }

            Table table = Casino.GetTable(tableTitle);

            if (table.IsFull) {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableFull);
                return;
            }

            writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.Success);
            writer.WriteLine(table.GetFirstFreeSeatIndex());
            writer.WriteLine(table.SmallBlind);
            writer.WriteLine(buyIn);

            if (table.IsEmpty) {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableEmpty);
            }
            else {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableNotEmpty);

                for (int i = 0; i < table.MaxPlayers; i++) {
                    if(!table.GetSeatAt(i).IsOccupied) continue;

                    Seat seat = table.GetSeatAt(i);
                    writer.WriteLine(seat.Player.Username);
                    writer.WriteLine(seat.Player.Stack);
                    break;
                }
            }
            
            Casino.MovePlayerFromLobbyToTable(username, table, buyIn);
        }
    }
}