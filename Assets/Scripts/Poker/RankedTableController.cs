using System.Linq;
using Dao;
using Poker.Players;

namespace Poker
{
    public class RankedTableController : TableController
    {
        public override bool IsRanked => true;
        private TablePlayer[] _originalPlayers;
        
        public RankedTableController(Table table, string title, int smallBlind) : base(table, title, smallBlind) { }

        protected override void Kick(TablePlayer player)
        {
            var position = Table.PlayerCount - 1;
            TransferPlayerToPosition(player, position);
            
            var sortedRatings = _originalPlayers.Select(p => DaoProvider.Dao.GetEloRating(p.Username)).ToList();

            var placeFinished = Table.PlayerCount;
            var oldRating = DaoProvider.Dao.GetEloRating(player.Username);
            var newRating = EloSystem.CalculateNewRatingForPosition(position, sortedRatings);
            DaoProvider.Dao.SetEloRating(player.Username, newRating);
            
            SendMatchResult(player.Client, placeFinished, oldRating, newRating);
            
            Casino.RemoveTablePlayer(player);
            Casino.AddLobbyPlayer(new LobbyPlayer(player.Client, player.ChipCount));
        }

        private void TransferPlayerToPosition(TablePlayer player, int position)
        {
            for (var i = 0; i < _originalPlayers.Length; i++)
            {
                if (_originalPlayers[i].Username != player.Username) continue;
                
                var transferringPlayer = _originalPlayers[i];
                _originalPlayers[i] = _originalPlayers[position];
                _originalPlayers[position] = transferringPlayer;
                break;
            }
        }

        private static void SendMatchResult(Client client, int placeFinished, int oldRating, int newRating)
        {
            new Client.Package(client)
                .Append(ServerResponse.LeaveTable)
                .Append(ServerResponse.LeaveTableRanked)
                .Append(placeFinished)
                .Append(oldRating)
                .Append(newRating)
                .Send();
        }
        
        protected override void OnPlayerJoined()
        {
            if (!IsLocked && Table.PlayerCount == Table.MaxPlayers)
            {
                IsLocked = true;
                _originalPlayers = Table.GetPlayerArray();
                StartNewRound();
            }
        }
        
        protected override void OnRoundFinished()
        {
            if (Table.PlayerCount == 1)
            {
                foreach (var player in Table) Kick(player);

                IsLocked = false;
                return;
            }

            if (Table.PlayerCount >= 2)
                StartNewRound();
        }
    }
}