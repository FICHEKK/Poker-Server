using System.IO;

namespace RequestProcessors {
    public interface IRequestProcessor {
        void ProcessRequest(StreamReader reader, StreamWriter writer);
    }
}