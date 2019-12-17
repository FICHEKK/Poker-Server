using System.IO;
using Poker;

namespace RequestProcessors {
    public class AllInRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string allInAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Seat.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerAllIn);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(allInAmount);
            
            dealer.Round.SeatAllIn(int.Parse(allInAmount));
        }
    }
}