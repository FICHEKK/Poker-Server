using System.IO;
using Poker;

namespace RequestProcessors {
    public class JoinTableRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string tableTitle = reader.ReadLine();

            if (!Casino.HasTableWithTitle(tableTitle)) {
                writer.BaseStream.WriteByte((byte) ServerResponse.JoinTableFailedTableDoesNotExist);
            }
            else if (Casino.GetTable(tableTitle).IsFull) {
                writer.BaseStream.WriteByte((byte) ServerResponse.JoinTableFailedTableFull);
            }
            else {
                Table table = Casino.GetTable(tableTitle);
                // TODO add player here
                writer.BaseStream.WriteByte((byte) ServerResponse.JoinTableSucceeded);
            }
        }
    }
}