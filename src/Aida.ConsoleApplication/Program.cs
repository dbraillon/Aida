using Aida.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aida.ConsoleApplication
{
    public class Program
    {
        static void Main(string[] args)
        {
            Voice voice = new Voice(CultureInfo.CurrentCulture);
            voice.Say("Bonjour ! Contente de vous revoir !");
        }
    }
}
