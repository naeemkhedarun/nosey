using System;

namespace CSNosey
{
    class Entry
    {
        public DateTime Date { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Query { get; set; }
        public int Port { get; set; }
        public int Duration { get; set; }
        public string SourceIP { get; set; }
        public string ClientIP { get; set; }
        public string UserAgent { get; set; }
        public string ClientToServerHost { get; set; }
        public int StatusCode { get; set; }
        public int SubStatus { get; set; }
        public int Win32Status { get; set; }
        public int ServerToClientBytes { get; set; }
        public int ClientToServerBytes { get; set; }
    }
}