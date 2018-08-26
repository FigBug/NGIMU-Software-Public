using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NgimuApi;
using NgimuApi.Logging;
using Rug.Osc;

namespace NgimuSDCardFileConverterCMD
{
    class Program
    {
        private long totalMaximumBytes = 0;
        private int lastProgress = 0;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: NgimuSDCardFileConverterCMD <source file> <dest dir>");
                return;
            }

            var program = new Program();
            program.go(args);
            Environment.Exit(0);
        }
        private void go(string[] args)
        {
            try
            {
                var path = args[0];

                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                {
                    Console.WriteLine("Source file does not exist");
                    return;
                }

                totalMaximumBytes += new FileInfo(path).Length;

                var connectionInfo = new SDCardFileConnectionInfo();
                connectionInfo.FilePath = path;

                var connection = new Connection(connectionInfo);

                var directory = args[1];
                var logger = new SessionLogger(SessionSettings.CreateForFileAndPath(directory, info), connection);

                logger.Start();

                connection.Message += new MessageEvent(ConnectionMessage);
                connection.Connect();

                logger.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        void ConnectionMessage(Connection source, MessageDirection direction, OscMessage message)
        {
            var progressBytes = source.CommunicationStatistics.BytesReceived.Total;
            var progress = (int)((progressBytes) / (double)(totalMaximumBytes) * 100);

            if (progress != lastProgress)
            {
                Console.WriteLine(progress);
                lastProgress = progress;
            }
        }
    }
}
