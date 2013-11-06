using System;
using PlainElastic.Net;

namespace CSNosey.Importers
{
    public abstract class ElasticImporter : IImporter
    {
        protected readonly IElasticConnection _connection;

        protected ElasticImporter(IElasticConnection connection)
        {
            _connection = connection;
        }

        public virtual void Import(DateTime @from)
        {
            throw new NotImplementedException();
        }
    }
}