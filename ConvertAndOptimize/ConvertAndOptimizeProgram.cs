using FindStuff;

namespace ConvertAndOptimize
{
    internal class ConvertAndOptimizeProgram
    {
        static void Main(string[] args)
        {
            //MediaOptimizer.Mp42Webm(Paths.BTD.Combine(Current.Country));
            //MediaOptimizer.OptimizeJpg(Paths.BTD.Combine(Current.Country));
            UpdatePositions(false);
            Console.WriteLine("done");
            Console.ReadLine();
        }

        public static void UpdatePositions(bool verbose)
        {
            foreach (var fm in FindStuff.Files.GetFiles(Paths.BTD.Combine(Current.Country))
                           .Select(f => f.Load()))
            {
                var location = FindStuff.LocationsFinder.FindForFile(fm.OriginalFile, verbose);

                if (location != null)
                {
                    if (!verbose) Console.Write("+");
                    fm.Pos = new Location() { Lat = location.Lat, Lon = location.Lon };
                    fm.Save();
                }
                else
                {
                    var closest = LocationsFinder.FindClosestToDateTime(fm.DateTime);
                    if (!verbose) Console.Write("+");
                    fm.Pos = new Location() { Lat = closest.Lat, Lon = closest.Lon };
                    fm.Save();
                    if (!verbose) Console.Write("-");
                }
            }
        }
    }
}