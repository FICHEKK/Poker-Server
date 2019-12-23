namespace Dao
{
    /// <summary>
    /// A provider of the Data Access Object implementation.
    /// </summary>
    public static class DaoProvider
    {
        /// <summary>
        /// Path of the database that will store all of the users.
        /// </summary>
        private const string DatabasePath = "Assets/Databases/clients.dat";

        /// <summary>
        /// The concrete Data Access Object implementation.
        /// </summary>
        public static IDao Dao { get; } = new FileDao(DatabasePath);
    }
}