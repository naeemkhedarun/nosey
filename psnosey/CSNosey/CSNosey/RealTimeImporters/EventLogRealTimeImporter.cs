using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reactive.Disposables;
using System.Xml;

namespace CSNosey.RealTimeImporters
{
    internal class EventLogRealTimeImporter : IRealTimeImporter, IDisposable
    {
        private readonly ITime _time;
        private IDbLogger _connection;
        private CompositeDisposable _disposable;

        public EventLogRealTimeImporter(ITime time)
        {
            _time = time;
        }

        public void Begin(IDbLogger connection)
        {
            _connection = connection;
            _disposable = new CompositeDisposable();

            var eventLogWatchers = new[] {"Application", "System"}.Select(s => new EventLogQuery(s, PathType.LogName)).Select(query => new EventLogWatcher(query));

            foreach (var eventLogWatcher in eventLogWatchers)
            {
                eventLogWatcher.EventRecordWritten += EventLogEventRead;
                eventLogWatcher.Enabled = true;
                _disposable.Add(eventLogWatcher);
            }
        }

        public void EventLogEventRead(object obj, EventRecordWrittenEventArgs arg)
        {
            if (arg.EventRecord != null)
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(arg.EventRecord.ToXml());

                var @event = new Event
                {
                    EventId = arg.EventRecord.Id, 
                    EventRecordId = arg.EventRecord.RecordId, 
                    LogName = arg.EventRecord.LogName, 
                    Message = string.Join(Environment.NewLine, arg.EventRecord.Properties.Select(property => property.Value.ToString())), 
                    Source = arg.EventRecord.ProviderName,
                    Date = arg.EventRecord.TimeCreated.Value.ToUniversalTime().ToString(_time.Format),
                    Level = arg.EventRecord.LevelDisplayName,
                    MachineName = Environment.MachineName
                };

                _connection.Log(@event);
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    public class Event
    {
        public string Date { get; set; }
        public string Message { get; set; }
        public long? EventRecordId { get; set; }
        public string LogName { get; set; }
        public int EventId { get; set; }
        public string Source { get; set; }
        public string Level { get; set; }
        public string MachineName { get; set; }
    }
}