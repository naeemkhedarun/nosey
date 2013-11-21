using System;
using System.Configuration;
using CSNosey.RealTimeImporters;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;

namespace CSNosey
{
    public interface IDbLogger
    {
        void Log(Event @event);
        void UpdateHeatbeat(string time);
        void LogCounter(Counter counter);
    }

    class ElasticSearchDbLogger : IDbLogger
    {
        private readonly ITime _time;
        private ElasticConnection _connection;
        private JsonNetSerializer _serializer;

        public ElasticSearchDbLogger(ITime time)
        {
            _time = time;
            var host = Environment.GetEnvironmentVariable("ElasticSearchHost");
            
            if (string.IsNullOrEmpty(host))
            {
                throw new ConfigurationErrorsException(@"Please define an environment variable of ElasticSearchHost with the hostname of your ES server.");
            }

            _connection = new ElasticConnection(host);
            _serializer = new JsonNetSerializer();

        }

        public void Log(Event @event)
        {
            var eventS = _serializer.Serialize(@event);
            _connection.Post(new IndexCommand("log", "event"), eventS);
        }

        public void UpdateHeatbeat(string time)
        {
            var beat = _serializer.Serialize(new ElasticHeartBeat { Date = _time.Now, MachineName = Environment.MachineName });
            _connection.Put(new IndexCommand("counter", "heartbeat", Environment.MachineName), beat);
        }

        public void LogCounter(Counter counter)
        {
            _connection.Post(new IndexCommand("counter", "machine"), _serializer.Serialize(counter));
        }

    }

    internal class ElasticHeartBeat
    {
        public string Date { get; set; }

        public string MachineName { get; set; }
    }

    class MongoDbLogger : IDbLogger
    {
        private MongoDatabase _connection;
        private MongoCollection<Event> _logCollection;
        private MongoCollection<MongoHeartBeat> _heartbeatCollection;
        private MongoCollection<Counter> _counterCollection;

        public MongoDbLogger()
        {
            _connection = new MongoClient("mongodb://localhost").GetServer().GetDatabase("nosey");
            _logCollection = _connection.GetCollection<Event>("log");
            _heartbeatCollection = _connection.GetCollection<MongoHeartBeat>("heartbeat");
            _counterCollection = _connection.GetCollection<Counter>("counter");
        }

        public void Log(Event @event)
        {
            _logCollection.Insert(@event);
        }

        public void UpdateHeatbeat(string time)
        {
            var heartBeat = _heartbeatCollection.FindOne(Query<MongoHeartBeat>.EQ(beat => beat.MachineName, Environment.MachineName));
            if (heartBeat == null)
            {
                _heartbeatCollection.Insert(new MongoHeartBeat{ MachineName = Environment.MachineName, LastHeartBeat = time});
            }
            else
            {
                heartBeat.LastHeartBeat = time;
                _heartbeatCollection.Save(heartBeat);    
            }
        }

        public void LogCounter(Counter counter)
        {
            _counterCollection.Insert(counter);
        }
    }

    internal class MongoHeartBeat
    {
        public ObjectId Id { get; set; }
        
        public string MachineName { get; set; }
        
        public string LastHeartBeat { get; set; }
    }

}