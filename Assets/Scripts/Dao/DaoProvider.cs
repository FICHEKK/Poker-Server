namespace Dao
{
    /// <summary>
    /// A provider of the Data Access Object implementation.
    /// </summary>
    public static class DaoProvider
    {
        /// <summary>
        /// The concrete Data Access Object implementation.
        /// </summary>
        public static IDao Dao { get; set; }
    }
}