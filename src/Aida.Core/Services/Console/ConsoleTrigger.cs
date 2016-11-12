using Aida.Core.Services.Console;
using System;

namespace Aida.Core.Triggers
{
    public class ConsoleTrigger : ITrigger
    {
        public ConsoleIngredient Read()
        {
            return new ConsoleIngredient()
            {
                Message = Console.ReadLine()
            };
        }
    }
}
