using Poker.EventArguments;

namespace Poker
{
    public abstract partial class TableController
    {
        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e)
        {
            switch (e.CurrentPhase)
            {
                case Round.Phase.Flop: RevealCommunityCards(ServerResponse.Flop, 3); break;
                case Round.Phase.Turn: RevealCommunityCards(ServerResponse.Turn, 1); break;
                case Round.Phase.River: RevealCommunityCards(ServerResponse.River, 1); break;
                case Round.Phase.Showdown: ProcessShowdown(); break;
                case Round.Phase.OnePlayerLeft: ProcessOnePlayerLeft(); break;
            }
        }
        
        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e)
        {
            SendBroadcastPackage(ServerResponse.PlayerIndex, e.CurrentPlayerIndex);

            new Client.Package(Table[e.CurrentPlayerIndex].Client)
                .Append(ServerResponse.RequiredBet)
                .Append(e.RequiredCall)
                .Append(e.MinRaise)
                .Append(e.MaxRaise)
                .Send();

            _decisionTimer.Stop();
            _decisionTimer.Start();
        }
    }
}