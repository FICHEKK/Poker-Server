using System.IO;
using Poker;

namespace RequestProcessors
{
    public class RaiseRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string raisedToAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerRaised);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(raisedToAmount);

            dealer.Round.PlayerRaised(int.Parse(raisedToAmount));
        }
    }
}