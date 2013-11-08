using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;

namespace CSNosey.RealTimeImporters
{
    class PerformanceCounterRealTimeImporter : IRealTimeImporter, IDisposable
    {
        private IDisposable _disposable;

        public void Begin(ElasticConnection connection)
        {
            var serializer = new JsonNetSerializer();
            
            var counters = new[]
                {
                    new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                    new PerformanceCounter("LogicalDisk", "% Free Space", "_Total"),
                    new PerformanceCounter("Memory", "% Committed Bytes In Use"),
                };

            counters.Select(counter => counter.NextValue());

            _disposable = Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(l =>
                {
                    foreach (var counter in counters)
                    {
                        var jsonData = new Counter
                            {
                                MachineName = Environment.MachineName, 
                                Name = string.Format("{0}/{1}/{2}", 
                                counter.CategoryName, 
                                counter.CounterName, counter.InstanceName), 
                                Value = counter.NextValue(),
                                Date = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")
                            };

                        connection.Post(new IndexCommand("counter", "machine"), serializer.Serialize(jsonData));
                    }
                });
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    class Counter
    {
        public string MachineName { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
        public string Date { get; set; }
    }
}