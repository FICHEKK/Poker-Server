using System.IO;
using Poker;

namespace RequestProcessors {
    public class CreateTableRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string tableTitle = reader.ReadLine();
            int smallBlind = int.Parse(reader.ReadLine());
            int maxPlayers = int.Parse(reader.ReadLine());

            if (Casino.HasTableWithTitle(tableTitle)) {
                writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.TitleTaken);
                return;
            }
            
            Casino.AddTable(tableTitle, new Table(smallBlind, maxPlayers));
            writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.Success);
        }
    }
}