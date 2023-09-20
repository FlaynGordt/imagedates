// See https://aka.ms/new-console-template for more information

using FindStuff;

var allFiles = Directory.EnumerateFiles(Paths.BTD.Combine(Current.Country), "*.*");

bool verbose = false;

foreach (var file in allFiles.Select(f => new FileInfo(f)))
{
    if (file.Extension.Contains("txt"))
    {
        continue;
    }

    var date = DateFinder.SearchDate(file, verbose);

    if (!date.HasValue)
    {
        if (verbose) Console.WriteLine("[?]" + file);
    }
    else
    {
        if (file.CreationTime != date)
        {
            Console.WriteLine("[U]" + file.Name + " -> " + date);
            file.CreationTime = date.Value;
        }
        else { if(verbose) Console.WriteLine("[S]" + file.Name + " -> " + date); }
    }
}

Console.ReadLine(); 
