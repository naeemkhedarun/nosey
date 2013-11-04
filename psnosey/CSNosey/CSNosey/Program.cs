using System;
using System.Collections.Generic;
using MSUtil;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;

namespace CSNosey
{
    class Program
    {
        static void Main(string[] args)
        {
            var logQuery = new LogQueryClass();
            var inputFormat = new COMW3CInputContextClass();
            var strQuery = string.Format(@"SELECT to_timestamp(date, time) as date, 
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
                                                        time-taken as duration FROM {0}", args[0]);

            ILogRecordset results = logQuery.Execute(strQuery, inputFormat);

            var tweets = new List<Entry>();

            while (!results.atEnd())
            {
                var logRecord = results.getRecord();

                var date = logRecord.getValue("date");
                var sourceIP = logRecord.getValue("sourceIP");
                var method = logRecord.getValue("method");
                var uri = logRecord.getValue("uri");
                var query = logRecord.getValue("query") is DBNull ? string.Empty : logRecord.getValue("query");
                var port = logRecord.getValue("port");
                var clientIP = logRecord.getValue("clientIP") is DBNull ? string.Empty : logRecord.getValue("clientIP");
                var userAgent = logRecord.getValue("userAgent") is DBNull ? string.Empty : logRecord.getValue("userAgent");
                var clientToServerHost = logRecord.getValue("clientToServerHost") is DBNull ? string.Empty : logRecord.getValue("clientToServerHost");
                var statusCode = logRecord.getValue("statusCode");
                var subStatus = logRecord.getValue("subStatus");
                var win32Status = logRecord.getValue("win32Status");
                var serverToClientBytes = logRecord.getValue("serverToClientBytes");
                var clientToServerBytes = logRecord.getValue("clientToServerBytes");
                var duration = logRecord.getValue("duration");

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

            var connection = new ElasticConnection("localhost");

            connection.Post(bulkCommand, bulkJson);
        }
    }
}
