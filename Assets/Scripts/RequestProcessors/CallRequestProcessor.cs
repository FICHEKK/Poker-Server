using System.IO;
using Poker;

namespace RequestProcessors
{
    public class CallRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string callAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerCalled);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(callAmount);

            dealer.Round.PlayerCalled(int.Parse(callAmount));
        }
    }
}