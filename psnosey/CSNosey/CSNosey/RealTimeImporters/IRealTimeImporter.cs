using PlainElastic.Net;

namespace CSNosey.RealTimeImporters
{
    internal interface IRealTimeImporter
    {
        void Begin(ElasticConnection connection);
    }
}