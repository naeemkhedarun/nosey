using System;
using System.Collections.Generic;
using MSUtil;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;

namespace CSNosey.Importers
{
    public class BulkRequestLogImporter : ElasticImporter
    {
        public BulkRequestLogImporter(IElasticConnection connection) : base(connection)
        {
        }

        public override void Import(DateTime @from)
        {
            var logQuery = new LogQueryClass();
            var inputFormat = new COMW3CInputContextClass();
            string strQuery = string.Format(@"SELECT to_timestamp(date, time) as date, 
                                                        s-ip as sourceIP, 
                                                        cs-method as method, 
                                                        cs-uri-stem as uri, 
                                                        cs-uri-query as query, 
                                                        s-port as port, 
                                                        c-ip as clientIP, 
                                                        cs(User-Agent) as userAgent, 
                                                        cs-host as clientToServerHost, 
                                                        sc-status as statusCode, 
                                                        sc-substatus as subStatus, 
                                                        sc-win32-status as win32Status, 
                                                        sc-bytes as serverToClientBytes, 
                                                        cs-bytes as clientToServerBytes, 
                                                        time-taken as duration FROM {0}", "SOME FILE");

            ILogRecordset results = logQuery.Execute(strQuery, inputFormat);

            var tweets = new List<Entry>();

            while (!results.atEnd())
            {
                ILogRecord logRecord = results.getRecord();

                dynamic date = logRecord.getValue("date");
                dynamic sourceIP = logRecord.getValue("sourceIP");
                dynamic method = logRecord.getValue("method");
                dynamic uri = logRecord.getValue("uri");
                dynamic query = logRecord.getValue("query") is DBNull ? string.Empty : logRecord.getValue("query");
                dynamic port = logRecord.getValue("port");
                dynamic clientIP = logRecord.getValue("clientIP") is DBNull
                                       ? string.Empty
                                       : logRecord.getValue("clientIP");
                dynamic userAgent = logRecord.getValue("userAgent") is DBNull
                                        ? string.Empty
                                        : logRecord.getValue("userAgent");
                dynamic clientToServerHost = logRecord.getValue("clientToServerHost") is DBNull
                                                 ? string.Empty
                                                 : logRecord.getValue("clientToServerHost");
                dynamic statusCode = logRecord.getValue("statusCode");
                dynamic subStatus = logRecord.getValue("subStatus");
                dynamic win32Status = logRecord.getValue("win32Status");
                dynamic serverToClientBytes = logRecord.getValue("serverToClientBytes");
                dynamic clientToServerBytes = logRecord.getValue("clientToServerBytes");
                dynamic duration = logRecord.getValue("duration");

                tweets.Add(new Entry
                    {
                        Date = date,
                        SourceIP = sourceIP,
                        Method = method,
                        Uri = uri,
                        Query = query,
                        Port = port,
                        ClientIP = clientIP,
                        UserAgent = userAgent,
                        ClientToServerHost = clientToServerHost,
                        StatusCode = statusCode,
                        SubStatus = subStatus,
                        Win32Status = win32Status,
                        ServerToClientBytes = serverToClientBytes,
                        ClientToServerBytes = clientToServerBytes,
                        Duration = duration
                    });

                results.moveNext();
            }

            var serializer = new JsonNetSerializer();
            string bulkCommand = new BulkCommand(index: "log", type: "iis");

            string bulkJson =
                new BulkBuilder(serializer)
                    .BuildCollection(tweets,
                                     (builder, tweet) => builder.Create(tweet)
                    );


            _connection.Post(bulkCommand, bulkJson);
        }
    }
}