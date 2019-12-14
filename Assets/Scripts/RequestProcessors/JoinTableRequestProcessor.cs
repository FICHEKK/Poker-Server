using System.Diagnostics;
using System.IO;
using System.Threading;
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

            if (table.IsEmpty) {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableEmpty);
            }
            else {
                writer.BaseStream.WriteByte((byte) ServerJoinTableResponse.TableNotEmpty);

                for (int i = 0; i < table.MaxPlayers; i++) {
                    if(table.IsSeatEmpty(i)) continue;
                    Player player = table.GetPlayerAt(i);
                    writer.WriteLine(player.Username);
                    writer.WriteLine(player.ChipCount);
                    break;
                }
                
                writer.Flush();
            }
            
            table.AddPlayer(new Player(username, buyIn, reader, writer), table.GetFirstFreeSeat());
        }
    }
}