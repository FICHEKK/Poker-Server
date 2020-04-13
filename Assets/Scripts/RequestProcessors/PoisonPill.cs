namespace RequestProcessors
{
    public class PoisonPill : IClientRequestProcessor
    {
        public bool CanWait => throw new System.NotImplementedException();
        
        public void ReadPayloadData(Client client)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessRequest()
        {
            throw new System.NotImplementedException();
        }
    }
}