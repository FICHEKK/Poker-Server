﻿using Poker;

namespace RequestProcessors
{
    public class CheckRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest() => Casino.GetTablePlayer(_client.Username).TableController.PlayerCheck();
    }
}