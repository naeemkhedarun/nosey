using System;

namespace CSNosey.RealTimeImporters
{
    internal interface ITime
    {
        string Now { get; }
        string Format { get; }
    }

    internal class Time : ITime
    {
        public string Now
        {
            get { return DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); }
        }

        public string Format
        {
            get { return "dd/MM/yyyy HH:mm:ss"; }
        }
    }
}