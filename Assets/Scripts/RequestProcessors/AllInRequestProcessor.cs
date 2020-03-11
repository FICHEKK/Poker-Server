using System.IO;
using Poker;

namespace RequestProcessors
{
    public class AllInRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string allInAmount = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerAllIn);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            dealer.Broadcast(allInAmount);

            dealer.Round.PlayerAllIn(int.Parse(allInAmount));
        }
    }
}