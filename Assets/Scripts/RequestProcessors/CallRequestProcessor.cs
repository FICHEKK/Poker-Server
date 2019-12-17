using System.IO;
using Poker;

namespace RequestProcessors {
    public class CallRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string callAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Seat.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerCalled);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(callAmount);
            
            dealer.Round.SeatCalled(int.Parse(callAmount));
        }
    }
}