using System.IO;

namespace RequestProcessors
{
    /// <summary>
    /// Models classes whose objects process a single specific client request.
    /// </summary>
    public interface IRequestProcessor
    {
        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="username">Username to apply the request for.</param>
        /// <param name="reader">Stream reader used to read incoming data.</param>
        /// <param name="writer">Stream writer used to write output data.</param>
        void ProcessRequest(string username, StreamReader reader, StreamWriter writer);
    }
}