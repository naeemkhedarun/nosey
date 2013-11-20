using System;
using System.Configuration;
using CSNosey.RealTimeImporters;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
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
        private ElasticConnection _connection;
        private JsonNetSerializer _serializer;

        public ElasticSearchDbLogger()
        {
            var myKey = Registry.CurrentUser.OpenSubKey(@"Software\Nosey", false);
            var missing = myKey == null;
            var host = string.Empty;

            if (!missing)
            {
                host = (String)myKey.GetValue("ElasticSearchHost");
                missing = host == null;
            }
            
            if (missing)
            {
                throw new ConfigurationErrorsException(@"Please define an elasticsearch host key at HKEY_LOCAL_MACHINE\SOFTWARE\Nosey called ElasticSearchHost with the machines hostname.");
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
            var beat = _serializer.Serialize(new ElasticHeartBeat { Date = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"), MachineName = Environment.MachineName });
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