using System;

namespace CSNosey.Importers
{
    internal interface IImporter
    {
        void Import(DateTime @from);
    }
}