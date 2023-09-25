// See https://aka.ms/new-console-template for more information

using FindStuff;

var allFiles = Directory.EnumerateFiles(Paths.BTD.Combine(Current.Country), "*.*");

bool verbose = true;

foreach (var file in allFiles.Select(f => new FileInfo(f)))
{
    if (file.Extension.Contains("json"))
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
        var fm = file.Load();
        
        if (fm.DateTime != date)
        {
            Console.WriteLine("[U]" + file.Name + " -> " + date);

            fm.DateTime = date.Value;
            fm.Save();

            file.CreationTime = date.Value;
        }
        else { if(verbose) Console.WriteLine("[S]" + file.Name + " -> " + date); }
    }
}
Console.WriteLine("done");
Console.ReadLine(); 
