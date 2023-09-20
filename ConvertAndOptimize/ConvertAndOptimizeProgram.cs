using FindStuff;

namespace ConvertAndOptimize
{
    internal class ConvertAndOptimizeProgram
    {
        static void Main(string[] args)
        {
            //MediaOptimizer.Mp42Webm(Paths.BTD.Combine(Current.Country));
            MediaOptimizer.OptimizeJpg(Paths.BTD.Combine(Current.Country));

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}