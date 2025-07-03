namespace AppCalculatrice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Calculatrice";
            Calculatrice calc = new Calculatrice();
            calc.AfficherExpression();
        }
    }
}
