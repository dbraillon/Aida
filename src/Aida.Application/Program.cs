using Aida.Core;
using Aida.Core.Voices.French;
using Roggle.Core;
using System;
using static Aida.Core.Constants;

namespace Aida.Application
{
    class Program
    {
        static AidaCore Aida { get; set; }

        static void Main(string[] args)
        {
#if Release
            // Initialize log system
            GRoggle.Use(
                new EventLogRoggle(
                    eventSourceName: AidaApplicationSourceName, eventLogName: AidaLogName,
                    acceptedLogLevels: RoggleLogLevel.Debug | RoggleLogLevel.Error | RoggleLogLevel.Info | RoggleLogLevel.Warning
                )
            );
#endif

            // Start main application thread
            Aida = new AidaCore(
                voice: new FrenchFemaleVoice()
            );
            Aida.Start();
            
            // Set this thread idle, waiting for user input
            while (Aida.IsRunning)
            {
                var userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "exit":
                        Aida.Stop();
                        break;

                    default:
                        Aida.Write(userInput);
                        break;
                }
            }
        }
    }
}
