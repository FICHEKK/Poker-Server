using Poker;

namespace RequestProcessors
{
    public class CallRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string callAmount = client.Reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerCalled);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(client.Username).ToString());
            dealer.Broadcast(callAmount);

            dealer.Round.PlayerCalled(int.Parse(callAmount));
        }
    }
}