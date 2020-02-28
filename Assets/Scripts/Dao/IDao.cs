namespace Dao
{
    /// <summary>
    /// Interface of the Data Access Object that specifies all of
    /// the functional requirements that an implementation should have. 
    /// </summary>
    public interface IDao
    {
        /// <summary>Performs the login operation and returns the result.</summary>
        /// <param name="username">The client's username.</param>
        /// <param name="password">The client's password.</param>
        /// <returns>True if log-in succeed, false otherwise.</returns>
        bool Login(string username, string password);

        /// <summary>Performs the register operation and returns the result.</summary>
        /// <param name="username">The client's username.</param>
        /// <param name="password">The client's password.</param>
        /// <returns>True if registration was successful, false otherwise.</returns>
        bool Register(string username, string password);

        /// <summary>Checks if the provided username is already registered.</summary>
        /// <param name="username">The username to be checked.</param>
        /// <returns>True if the username is already registered, false otherwise.</returns>
        bool IsRegistered(string username);

        /// <summary>Checks if the client with the provided username is banned.</summary>
        /// <param name="username">The username to be checked.</param>
        /// <returns>True if the username is banned, false otherwise.</returns>
        bool IsBanned(string username);

        /// <summary>Fetches and returns the chip count of the client with the provided username.</summary>
        /// <param name="username">The client's username.</param>
        /// <returns>Client's bankroll or -1 if the provided username is invalid.</returns>
        int GetChipCount(string username);

        /// <summary>Fetches and returns the win count of the client with the provided username.</summary>
        /// <param name="username">The client's username.</param>
        /// <returns>Client's win count or -1 if the provided username is invalid.</returns>
        int GetWinCount(string username);

        /// <summary>Sets the chip count of the client with the provided username.</summary>
        /// <param name="username">The client's username.</param>
        /// <param name="chipCount">The client's new bankroll.</param>
        /// <returns>True if setting was successful, false otherwise.</returns>
        bool SetChipCount(string username, int chipCount);

        /// <summary>Sets the win count of the client with the provided username.</summary>
        /// <param name="username">The client's username.</param>
        /// <param name="winCount">The client's new win count.</param>
        /// <returns>True if setting was successful, false otherwise.</returns>
        bool SetWinCount(string username, int winCount);

        /// <summary>Sets the ban status of the client with the provided username.</summary>
        /// <param name="username">The client's username.</param>
        /// <param name="isBanned">The client's new ban status.</param>
        /// <returns>True if setting was successful, false otherwise.</returns>
        bool SetIsBanned(string username, bool isBanned);
    }
}