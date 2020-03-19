using Poker;

namespace RequestProcessors
{
    public class RaiseRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string raisedToAmount = client.Reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerRaised);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(client.Username).ToString());
            dealer.Broadcast(raisedToAmount);

            dealer.Round.PlayerRaised(int.Parse(raisedToAmount));
        }
    }
}