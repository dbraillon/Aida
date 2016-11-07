using Aida.Core;
using Aida.Core.Voices.French;
using Roggle.Core;
using System;
using static Aida.Core.Constants;

namespace Aida.Application
{
    class Program
    {
        static AidaCore CoreThread { get; set; }

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
            CoreThread = new AidaCore(
                voice: new FrenchFemaleVoice()
            );
            CoreThread.Start();
            
            // Set this thread idle, waiting for user input
            while (CoreThread.IsRunning)
            {
                var userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "exit":
                        CoreThread.Stop();
                        break;

                    default:
                        CoreThread.Write(userInput);
                        break;
                }
            }
        }
    }
}
