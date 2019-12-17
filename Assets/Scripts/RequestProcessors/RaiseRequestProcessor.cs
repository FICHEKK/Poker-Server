using System.IO;
using Poker;

namespace RequestProcessors {
    public class RaiseRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string raiseAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Seat.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerRaised);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(raiseAmount);
            
            dealer.Round.SeatRaised(int.Parse(raiseAmount));
        }
    }
}