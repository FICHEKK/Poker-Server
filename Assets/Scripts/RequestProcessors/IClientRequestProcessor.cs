namespace RequestProcessors
{
    /// <summary>
    /// Models classes whose objects process a single specific client request.
    /// </summary>
    public interface IClientRequestProcessor
    {
        /// <summary>
        /// Indicates whether the processing of this request can wait.
        /// If it can wait, this request will be delegated to the blocking queue.
        /// If it can't wait, the processing will always be performed immediately.
        /// </summary>
        bool CanWait { get; }
        
        /// <summary>
        /// Reads the payload data from the client request.
        /// </summary>
        void ReadPayloadData(Client client);
        
        /// <summary>Processes the request from the client.</summary>
        void ProcessRequest();
    }
}