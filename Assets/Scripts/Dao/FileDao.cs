using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Dao
{
    public class FileDao : IDao
    {
        private const int DefaultChipCount = 1000;
        private const int DefaultWinCount = 0;
        private const bool DefaultBanStatus = false;
        private const int RewardIntervalInHours = 4;
        private const int DefaultEloRating = 1000;

        private const int ExpectedLinePartCount = 7;
        private const char Separator = ',';
        private static readonly CultureInfo RewardTimestampCulture = new CultureInfo("en-GB");

        private readonly object _fileLock = new object();
        private readonly Dictionary<string, ClientData> _clients = new Dictionary<string, ClientData>();
        private readonly string _filePath;

        #region Public API

        public FileDao(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            var fileDirectory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            if (File.Exists(filePath))
            {
                LoadClientDictionary(filePath);
            }
            else
            {
                File.Create(filePath).Close();
            }
        }

        public bool Login(string username, string password)
        {
            return IsRegistered(username) && _clients[username].Password == password;
        }

        public bool Register(string username, string password)
        {
            if (IsRegistered(username)) return false;

            var clientData = new ClientData(
                username: username,
                password: password,
                chipCount: DefaultChipCount,
                winCount: DefaultWinCount, 
                isBanned: DefaultBanStatus,
                rewardTimestamp: DateTime.Now.AddHours(RewardIntervalInHours),
                eloRating: DefaultEloRating
            );

            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(_filePath, clientData + Environment.NewLine);
                    _clients.Add(username, clientData);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsRegistered(string username)
        {
            return _clients.ContainsKey(username);
        }

        public bool IsBanned(string username)
        {
            return IsRegistered(username) && _clients[username].IsBanned;
        }

        public int GetChipCount(string username)
        {
            return IsRegistered(username) ? _clients[username].ChipCount : -1;
        }

        public int GetWinCount(string username)
        {
            return IsRegistered(username) ? _clients[username].WinCount : -1;
        }

        public DateTime? GetRewardTimestamp(string username)
        {
            return IsRegistered(username) ? _clients[username].RewardTimestamp : (DateTime?) null;
        }

        public int GetEloRating(string username)
        {
            return IsRegistered(username) ? _clients[username].EloRating : -1;
        }

        public void SetChipCount(string username, int chipCount)
        {
            if (!IsRegistered(username)) return;

            ClientData data = _clients[username].Clone();
            data.ChipCount = chipCount;
            UpdateClientData(data);
        }

        public void SetWinCount(string username, int winCount)
        {
            if (!IsRegistered(username)) return;

            ClientData data = _clients[username].Clone();
            data.WinCount = winCount;
            UpdateClientData(data);
        }

        public void SetIsBanned(string username, bool isBanned)
        {
            if (!IsRegistered(username)) return;

            ClientData data = _clients[username].Clone();
            data.IsBanned = isBanned;
            UpdateClientData(data);
        }

        public void UpdateRewardTimestamp(string username)
        {
            if (!IsRegistered(username)) return;

            ClientData data = _clients[username].Clone();
            data.RewardTimestamp = DateTime.Now.AddHours(RewardIntervalInHours);
            UpdateClientData(data);
        }

        public void SetEloRating(string username, int eloRating)
        {
            if (!IsRegistered(username)) return;

            ClientData data = _clients[username].Clone();
            data.EloRating = eloRating;
            UpdateClientData(data);
        }

        #endregion

        #region Private helper methods

        private void LoadClientDictionary(string filePath)
        {
            foreach (string line in File.ReadAllLines(filePath))
            {
                ClientData data = ParseLine(line);
                if (data == null) continue;

                _clients.Add(data.Username, data);
            }
        }

        private static ClientData ParseLine(string line)
        {
            if (line == null) return null;

            string[] parts = line.Split(Separator);
            if (parts.Length != ExpectedLinePartCount) return null;
            
            try
            {
                string username = parts[0];
                string password = parts[1];
                int chipCount = int.Parse(parts[2]);
                int winCount = int.Parse(parts[3]);
                bool isBanned = bool.Parse(parts[4]);
                DateTime rewardTimestamp = DateTime.Parse(parts[5]);
                int eloRating = int.Parse(parts[6]);
                
                return new ClientData(username, password, chipCount, winCount, isBanned, rewardTimestamp, eloRating);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Updates client data, both in the internal cache and in the
        /// persistent memory (file).
        /// </summary>
        /// <param name="data">New client's data.</param>
        private void UpdateClientData(ClientData data)
        {
            if (!IsRegistered(data.Username)) return;
            
            lock (_fileLock)
            {
                string[] lines = File.ReadAllLines(_filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].StartsWith(data.Username)) continue;
                    lines[i] = data.ToString();
                    break;
                }

                File.WriteAllLines(_filePath, lines);
            }
            
            _clients[data.Username] = data;
        }

        #endregion

        private class ClientData
        {
            public string Username { get; }
            public string Password { get; }
            public int ChipCount { get; set; }
            public int WinCount { get; set; }
            public bool IsBanned { get; set; }
            public DateTime RewardTimestamp { get; set; }
            public int EloRating { get; set; }

            public ClientData(string username, string password, int chipCount, int winCount, bool isBanned, DateTime rewardTimestamp, int eloRating)
            {
                Username = username;
                Password = password;
                ChipCount = chipCount;
                WinCount = winCount;
                IsBanned = isBanned;
                RewardTimestamp = rewardTimestamp;
                EloRating = eloRating;
            }

            public ClientData Clone()
            {
                return new ClientData(Username, Password, ChipCount, WinCount, IsBanned, RewardTimestamp, EloRating);
            }

            public override string ToString()
            {
                return Username + Separator +
                       Password + Separator +
                       ChipCount + Separator +
                       WinCount + Separator +
                       IsBanned + Separator +
                       RewardTimestamp.ToString(RewardTimestampCulture) + Separator +
                       EloRating;
            }
        }
    }
}