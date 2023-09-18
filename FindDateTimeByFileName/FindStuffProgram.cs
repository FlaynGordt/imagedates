namespace FindStuff
{

    public class FindStuffProgram
    {
        // Uncomment the following line to resolve.
        static void Main(string[] args)
        {
            foreach (var file in args.Select(f => new FileInfo(f)))
            {
                var res = DateFinder.SearchDate(file);

                if (res.HasValue)
                {
                    Console.WriteLine(new DateTimeOffset(res.Value).ToUnixTimeSeconds().ToString());
                }
            }
        }
    }


}
