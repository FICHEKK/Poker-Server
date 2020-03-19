using Poker;

namespace RequestProcessors
{
    public class AllInRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string allInAmount = client.Reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerAllIn);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(client.Username).ToString());
            dealer.Broadcast(allInAmount);

            dealer.Round.PlayerAllIn(int.Parse(allInAmount));
        }
    }
}