using Aida.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida.Application
{
    class Program
    {
        static Thread ApplicationThread { get; set; }
        static volatile bool IsRunning;

        static void Main(string[] args)
        {
            IsRunning = true;
            ApplicationThread = new Thread(Loop);
            ApplicationThread.Start();
            
            while (IsRunning)
            {
                var userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "exit":
                        IsRunning = false;
                        break;

                    default:
                        break;
                }
            }

            ApplicationThread.Join();
        }

        static void Loop()
        {
            Voice v = new Voice(CultureInfo.CurrentCulture);
            v.Say("Hello! I'm glad to see you back!");

            while (IsRunning)
            {
                Thread.Sleep(1000);
            }

            v.Say("Goodbye!");
        }
    }
}
