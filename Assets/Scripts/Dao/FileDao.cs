using System;
using System.Collections.Generic;
using System.IO;

namespace Dao {
    public class FileDao : IDao {

        public string FilePath { get; }

        private const int ExpectedLinePartCount = 4;
        private const char Separator = '\t';
        private readonly Dictionary<string, UserData> _users = new Dictionary<string, UserData>();

        #region Public API
        
        public FileDao(string filePath) {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (File.Exists(filePath)) {
                LoadUsersDictionary(filePath);
            }
            else {
                File.Create(filePath).Close();
            }
        }

        public bool Login(string username, string password) {
            return IsRegistered(username) && _users[username].Password == password;
        }

        public bool Register(string username, string password) {
            if (IsRegistered(username)) return false;

            UserData userData = new UserData(username, password, 0, 0);

            try {
                File.AppendAllText(FilePath, userData + Environment.NewLine);
                _users.Add(username, userData);
                return true;
            }
            catch {
                return false;
            }
        }

        public bool IsRegistered(string username) {
            return _users.ContainsKey(username);
        }

        public int GetChipCount(string username) {
            return IsRegistered(username) ? _users[username].ChipCount : -1;
        }

        public int GetWinCount(string username) {
            return IsRegistered(username) ? _users[username].WinCount : -1;
        }
        
        public bool SetChipCount(string username, int chipCount) {
            if (!IsRegistered(username)) return false;
            
            UserData data = _users[username].Clone();
            data.ChipCount = chipCount;
            return UpdateUserData(data);
        }

        public bool SetWinCount(string username, int winCount) {
            if (!IsRegistered(username)) return false;

            UserData data = _users[username].Clone();
            data.WinCount = winCount;
            return UpdateUserData(data);
        }
        
        #endregion

        #region Private helper methods
        
        private void LoadUsersDictionary(string filePath) {
            foreach (string line in File.ReadAllLines(filePath)) {
                UserData data = ParseLine(line);
                if(data == null) continue;
                    
                _users.Add(data.Username, data);
            }
        }

        private UserData ParseLine(string line) {
            if (line == null) return null;

            string[] parts = line.Split(Separator);
            if (parts.Length != ExpectedLinePartCount) return null;

            string username = parts[0];
            string password = parts[1];
            int chipCount = int.Parse(parts[2]);
            int winCount = int.Parse(parts[3]);
            
            return new UserData(username, password, chipCount, winCount);
        }

        /// <summary>
        /// Updates user data, both in the internal cache and in the
        /// persistent memory (file).
        /// </summary>
        /// <param name="data">New user's data.</param>
        /// <returns>True if updating was successful, false otherwise.</returns>
        private bool UpdateUserData(UserData data) {
            bool fileSuccessfullyUpdated = UpdateUserDataFile(data);

            if (!fileSuccessfullyUpdated) return false;
            
            _users[data.Username] = data;
            return true;
        }
        
        /// <summary>
        /// Updates the specified user's data in the persistent memory (file).
        /// </summary>
        /// <param name="data">New user's data.</param>
        /// <returns>True if updating was successful, false otherwise.</returns>
        private bool UpdateUserDataFile(UserData data) {
            if (!IsRegistered(data.Username)) return false;

            try {
                string[] lines = File.ReadAllLines(FilePath);
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].StartsWith(data.Username)) {
                        lines[i] = data.ToString();
                        break;
                    }
                }
                File.WriteAllLines(FilePath, lines);
                return true;
            }
            catch {
                return false;
            }
        }

        #endregion

        private class UserData {
            public string Username { get; set; }
            public string Password { get; set; }
            public int ChipCount { get; set; }
            public int WinCount { get; set; }

            public UserData(string username, string password, int chipCount, int winCount) {
                Username = username;
                Password = password;
                ChipCount = chipCount;
                WinCount = winCount;
            }
            
            public UserData Clone() {
                return new UserData(Username, Password, ChipCount, WinCount);
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