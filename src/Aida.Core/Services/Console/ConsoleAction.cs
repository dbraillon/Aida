namespace Aida.Core.Services.Console
{
    public class ConsoleAction : IAction
    {
        public void Write(ConsoleIngredient ingredient)
        {
            System.Console.WriteLine(ingredient.Message);
        }
    }
}
