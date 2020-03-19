using Poker;

namespace RequestProcessors
{
    public class CheckRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            Dealer dealer = Casino.GetTablePlayer(client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerChecked);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(client.Username).ToString());

            dealer.Round.PlayerChecked();
        }
    }
}