using Aida.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aida.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            Voice v = new Voice(CultureInfo.CurrentCulture);
            v.Say("Hello! I'm glad to see you back!");
        }
    }
}
