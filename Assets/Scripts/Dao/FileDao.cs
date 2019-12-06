using System;
using System.Collections.Generic;
using System.IO;

namespace Dao {
    public class FileDao : IDao {

        public string FilePath { get; }

        private const int ExpectedLinePartCount = 4;
        private const char Separator = '\t';
        
        private readonly object _fileLock = new object();
        private readonly Dictionary<string, ClientData> _clients = new Dictionary<string, ClientData>();

        #region Public API
        
        public FileDao(string filePath) {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (File.Exists(filePath)) {
                LoadClientDictionary(filePath);
            }
            else {
                File.Create(filePath).Close();
            }
        }

        public bool Login(string username, string password) {
            return IsRegistered(username) && _clients[username].Password == password;
        }

        public bool Register(string username, string password) {
            if (IsRegistered(username)) return false;

            ClientData clientData = new ClientData(username, password, 0, 0);

            try {
                lock (_fileLock) {
                    File.AppendAllText(FilePath, clientData + Environment.NewLine);
                    _clients.Add(username, clientData);
                }

                return true;
            }
            catch {
                return false;
            }
        }

        public bool IsRegistered(string username) {
            return _clients.ContainsKey(username);
        }

        public int GetChipCount(string username) {
            return IsRegistered(username) ? _clients[username].ChipCount : -1;
        }

        public int GetWinCount(string username) {
            return IsRegistered(username) ? _clients[username].WinCount : -1;
        }
        
        public bool SetChipCount(string username, int chipCount) {
            if (!IsRegistered(username)) return false;
            
            ClientData data = _clients[username].Clone();
            data.ChipCount = chipCount;
            return UpdateClientData(data);
        }

        public bool SetWinCount(string username, int winCount) {
            if (!IsRegistered(username)) return false;

            ClientData data = _clients[username].Clone();
            data.WinCount = winCount;
            return UpdateClientData(data);
        }
        
        #endregion

        #region Private helper methods
        
        private void LoadClientDictionary(string filePath) {
            foreach (string line in File.ReadAllLines(filePath)) {
                ClientData data = ParseLine(line);
                if(data == null) continue;
                    
                _clients.Add(data.Username, data);
            }
        }

        private ClientData ParseLine(string line) {
            if (line == null) return null;

            string[] parts = line.Split(Separator);
            if (parts.Length != ExpectedLinePartCount) return null;

            string username = parts[0];
            string password = parts[1];
            int chipCount = int.Parse(parts[2]);
            int winCount = int.Parse(parts[3]);
            
            return new ClientData(username, password, chipCount, winCount);
        }

        /// <summary>
        /// Updates client data, both in the internal cache and in the
        /// persistent memory (file).
        /// </summary>
        /// <param name="data">New client's data.</param>
        /// <returns>True if updating was successful, false otherwise.</returns>
        private bool UpdateClientData(ClientData data) {
            bool fileSuccessfullyUpdated = UpdateClientDataFile(data);

            if (!fileSuccessfullyUpdated) return false;
            
            _clients[data.Username] = data;
            return true;
        }
        
        /// <summary>
        /// Updates the specified client's data in the persistent memory (file).
        /// </summary>
        /// <param name="data">New client's data.</param>
        /// <returns>True if updating was successful, false otherwise.</returns>
        private bool UpdateClientDataFile(ClientData data) {
            if (!IsRegistered(data.Username)) return false;

            try {
                lock (_fileLock) {
                    string[] lines = File.ReadAllLines(FilePath);
                    for (int i = 0; i < lines.Length; i++) {
                        if (!lines[i].StartsWith(data.Username)) continue;
                        lines[i] = data.ToString();
                        break;
                    }
                    File.WriteAllLines(FilePath, lines);
                }
                return true;
            }
            catch {
                return false;
            }
        }

        #endregion

        private class ClientData {
            public string Username { get; }
            public string Password { get; }
            public int ChipCount { get; set; }
            public int WinCount { get; set; }

            public ClientData(string username, string password, int chipCount, int winCount) {
                Username = username;
                Password = password;
                ChipCount = chipCount;
                WinCount = winCount;
            }
            
            public ClientData Clone() {
                return new ClientData(Username, Password, ChipCount, WinCount);
            }

            public override string ToString() {
                return Username + Separator +
                       Password + Separator +
                       ChipCount + Separator +
                       WinCount;
            }
        }
    }
}