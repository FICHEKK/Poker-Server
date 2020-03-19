namespace RequestProcessors
{
    /// <summary>
    /// Models classes whose objects process a single specific client request.
    /// </summary>
    public interface IRequestProcessor
    {
        /// <summary>Processes the request from the client.</summary>
        /// <param name="client">Client that is performing the request.</param>
        void ProcessRequest(Client client);
    }
}