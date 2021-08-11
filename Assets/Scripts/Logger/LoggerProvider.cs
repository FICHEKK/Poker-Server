namespace Logger
{
    /// <summary>
    /// A provider of the Logger implementation.
    /// </summary>
    public static class LoggerProvider
    {
        /// <summary>The concrete Logger implementation.</summary>
        public static ILogger Logger { get; set; }
    }
}