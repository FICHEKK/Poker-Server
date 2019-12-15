using System.IO;
using Poker;

namespace RequestProcessors {
    public class TableListRequestProcessor : IRequestProcessor{
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            writer.WriteLine(Casino.TableCount);
        
            foreach (string tableName in Casino.TableNames) {
                Table table = Casino.GetTable(tableName);
            
                writer.WriteLine(tableName);
                writer.WriteLine(table.SmallBlind);
                writer.WriteLine(table.PlayerCount);
                writer.WriteLine(table.MaxPlayers);
            }
        }
    }
}