// See https://aka.ms/new-console-template for more information
using FindStuff;

string filename = "";
do
{
    Console.Write("filename with extension: ");
    filename = Console.ReadLine();

    Console.WriteLine(DateFinder.SearchDate(new FileInfo(filename), true));

    Console.ReadLine();
} while (!string.IsNullOrWhiteSpace(filename));

