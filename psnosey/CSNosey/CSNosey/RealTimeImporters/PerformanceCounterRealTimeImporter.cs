using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace CSNosey.RealTimeImporters
{
    class PerformanceCounterRealTimeImporter : IRealTimeImporter, IDisposable
    {
        private readonly ITime _time;
        private IDisposable _disposable;

        public PerformanceCounterRealTimeImporter(ITime time)
        {
            _time = time;
        }

        public void Begin(IDbLogger connection)
        {
            var counters = new[]
                {
                    new KeyValuePair<string, PerformanceCounter>("CpuTime", new PerformanceCounter("Processor", "% Processor Time", "_Total")),
                    new KeyValuePair<string, PerformanceCounter>("FreeSpace", new PerformanceCounter("LogicalDisk", "% Free Space", "_Total")),
                    new KeyValuePair<string, PerformanceCounter>("MemoryInUse", new PerformanceCounter("Memory", "% Committed Bytes In Use"))
                };

            counters.Select(counter => counter.Value.NextValue());

            _disposable = Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(l =>
                {
                    foreach (var counter in counters)
                    {
                        var jsonData = new Counter
                            {
                                MachineName = Environment.MachineName, 
                                Name = string.Format("{0}_{1}_{2}", counter.Value.CategoryName, counter.Value.CounterName, counter.Value.InstanceName),
                                StatName = counter.Key,
                                Value = counter.Value.NextValue(),
                                Date = _time.Now
                            };

                        connection.LogCounter(jsonData);
                    }
                });
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    public class Counter
    {
        public string StatName { get; set; }
        public string MachineName { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
        public string Date { get; set; }
    }
}