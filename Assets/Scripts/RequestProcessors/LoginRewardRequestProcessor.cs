using System;
using Dao;
using Poker;

namespace RequestProcessors
{
    public class LoginRewardRequestProcessor : IClientRequestProcessor
    {
        private const float MinRewardPercentage = 0.03f;
        private const float MaxRewardPercentage = 0.10f;
        private const int MinRewardValue = 100;
        private const int MaxRewardValue = 1_000_000;

        public bool CanWait => false;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            if (DateTime.Now >= DaoProvider.Dao.GetRewardTimestamp(_client.Username))
            {
                int reward = CalculateReward(_client.Username);
                
                DaoProvider.Dao.SetChipCount(_client.Username, DaoProvider.Dao.GetChipCount(_client.Username) + reward);
                DaoProvider.Dao.UpdateRewardTimestamp(_client.Username);

                Casino.GetLobbyPlayer(_client.Username).ChipCount += reward;
                
                _client.Writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardActive);
                _client.Writer.WriteLine(reward);
            }
            else
            {
                _client.Writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardNotActive);
                TimeSpan? timeUntilReward = DaoProvider.Dao.GetRewardTimestamp(_client.Username) - DateTime.Now;
                _client.Writer.WriteLine(timeUntilReward?.ToString(@"hh\:mm"));
            }
        }

        private static int CalculateReward(string username)
        {
            int clientChipCount = DaoProvider.Dao.GetChipCount(username);
            int minReward = (int) (clientChipCount * MinRewardPercentage);
            int maxReward = (int) (clientChipCount * MaxRewardPercentage);
            int randomReward = new Random().Next(minReward, maxReward);
            
            return Math.Max(MinRewardValue, Math.Min(randomReward, MaxRewardValue));
        }
    }
}