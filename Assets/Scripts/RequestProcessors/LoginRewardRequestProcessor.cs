﻿using System;
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
        
        public void ProcessRequest(Client client)
        {
            if (DateTime.Now >= DaoProvider.Dao.GetRewardTimestamp(client.Username))
            {
                int reward = CalculateReward(client.Username);
                
                DaoProvider.Dao.SetChipCount(client.Username, DaoProvider.Dao.GetChipCount(client.Username) + reward);
                DaoProvider.Dao.UpdateRewardTimestamp(client.Username);

                Casino.GetLobbyPlayer(client.Username).ChipCount += reward;
                
                client.Writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardActive);
                client.Writer.WriteLine(reward);
            }
            else
            {
                client.Writer.BaseStream.WriteByte((byte) ServerResponse.LoginRewardNotActive);
                TimeSpan? timeUntilReward = DaoProvider.Dao.GetRewardTimestamp(client.Username) - DateTime.Now;
                client.Writer.WriteLine(timeUntilReward?.ToString(@"hh\:mm"));
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