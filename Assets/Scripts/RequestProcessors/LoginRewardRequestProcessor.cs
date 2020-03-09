using System;
using System.IO;
using Dao;
using Poker;

namespace RequestProcessors
{
    public class LoginRewardRequestProcessor : IRequestProcessor
    {
        private const float MinRewardPercentage = 0.03f;
        private const float MaxRewardPercentage = 0.10f;
        private const int MinRewardValue = 100;
        private const int MaxRewardValue = 1_000_000;
        
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            if (DateTime.Now >= DaoProvider.Dao.GetRewardTimestamp(username))
            {
                int reward = CalculateReward(username);
                
                DaoProvider.Dao.SetChipCount(username, DaoProvider.Dao.GetChipCount(username) + reward);
                DaoProvider.Dao.UpdateRewardTimestamp(username);

                Casino.GetLobbyPlayer(username).ChipCount += reward;
                
                writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardActive);
                writer.WriteLine(reward);
            }
            else
            {
                writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardNotActive);
                TimeSpan? timeUntilReward = DaoProvider.Dao.GetRewardTimestamp(username) - DateTime.Now;
                writer.WriteLine(timeUntilReward?.ToString(@"hh\:mm"));
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