using FindStuff;
using System.Diagnostics;
using System.Text;

namespace GenerateSlides
{
    internal class GenerateSlidesProgram
    {
        static void Main(string[] args)
        {
            Country country = Current.Country;
            GenerateSlides(country, true);
        }

        private static void GenerateSlides(Country country, bool verbose)
        {

            string[] extensions = new string[] { "jpg", "webm", "png" };

            IEnumerable<string> relevantFiles = Enumerable.Empty<string>();

            foreach (var extension in extensions)
            {
                relevantFiles = relevantFiles.Concat(Directory.GetFiles(Paths.BTD.Combine(country), "*." + extension));
            }

            var files = relevantFiles.Select(f => new FileInfo(f)).OrderBy(f => f.CreationTime)
                                     .Where(f => country != Country.Ecuador || CountryFilter(f.CreationTime, country)) // für ecuador nach datum filtern
                                     .ToArray();

            DirectoryInfo outputDirectory = new DirectoryInfo(Paths.SLIDY_OUT.Combine(country));

            PrepareDirectoriesAndCopyFiles(outputDirectory);
            DirectoryInfo outputImageFilesDirectory = Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, "files"));

            List<SlideEntry> slideEntries = GenerateSlideEntries(files, outputImageFilesDirectory);

            var generatedDivsText = GenerateHTMLDivs(slideEntries, verbose);

            var htmlTextGenerated = File.ReadAllText(FindStuff.Paths.HTML_TEMPATE);
            htmlTextGenerated = htmlTextGenerated.Replace("SLIDESPLACEHOLDER", generatedDivsText);
            htmlTextGenerated = htmlTextGenerated.Replace("COVERIMAGEPLACEHOLDER", country.ToString().ToLower() + ".jpg");
            htmlTextGenerated = htmlTextGenerated.Replace("TITELPLACEHOLDER", country.ToString().ToUpper());

            File.WriteAllText(Path.Combine(outputDirectory.FullName, "out.html"), htmlTextGenerated);

            Console.WriteLine(slideEntries.Count + " Slides. Press 'o' to open or any other key to close");
            if (Console.ReadKey(true).KeyChar == 'o')
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(Path.Combine(outputDirectory.FullName, "out.html"))
                {
                    UseShellExecute = true
                };
                p.Start();
            }
        }

        private static bool CountryFilter(DateTime creationTime, Country country)
        {
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;

            switch (country)
            {
                case Country.Ecuador:
                    maxDate = new DateTime(2022, 08, 09);
                    break;
                case Country.Peru:
                    minDate = new DateTime(2022, 08, 09);
                    maxDate = DateTime.MaxValue;
                    break;
                case Country.Bolivia:
                    break;
                case Country.Chile:
                    break;
                case Country.Argentina:
                    break;
                case Country.NewZealand:
                    break;
                case Country.Australia:
                    break;
                case Country.Indonesia:
                    break;
                default:
                    break;
            }

            return (minDate < creationTime && creationTime < maxDate);

        }

        private static void PrepareDirectoriesAndCopyFiles(DirectoryInfo outputDirectory)
        {
            if (outputDirectory.Exists)
            {
                outputDirectory.Delete(true);
            }

            outputDirectory.Create();

           
            foreach (var resourceFile in Directory.EnumerateFiles(FindStuff.Paths.HTML_FILES).Select(f => new FileInfo(f)))
            {
                resourceFile.CopyTo(Path.Combine(outputDirectory.FullName, resourceFile.Name));
            }
        }

        private static string GenerateHTMLDivs(List<SlideEntry> slideEntries, bool verbose)
        {
            var sb = new StringBuilder();
            int counter = 0;
            foreach (var slideEntry in slideEntries)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine(++counter + " " + slideEntry.Date.ToString("ddMMyyyy") + " " + slideEntry.ImageFile.Name);
                var relativeFileName = "files/" + slideEntry.ImageFile.Name;

                sb.AppendLine("<div class=\"slide container hidden\">");
                sb.AppendLine($"  <h1>{slideEntry.Titel}" +
                                        $"</br>{slideEntry.ImageFile.CreationTime.ToString("dd.MM.yyyy")}" +
                                "</h1> ");

                if (slideEntry.ImageFile.FullName.ToLower().EndsWith("webm"))
                {
                    sb.AppendLine($"       <video src = \"{relativeFileName}\" controls autoplay muted loop lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }
                else if (slideEntry.ImageFile.FullName.ToLower().EndsWith("mp4"))
                {
                    sb.AppendLine($"       <video src = \"{relativeFileName}\" controls autoplay muted loop lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }
                else
                {
                    sb.AppendLine($"       <img src=\"{relativeFileName}\" lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }

                if (!string.IsNullOrWhiteSpace(slideEntry.Quote))
                {
                    sb.AppendLine($"<div class=\"bottom-right\">{slideEntry.Quote}</div>");
                }

                if (string.IsNullOrWhiteSpace(slideEntry.Link))
                {
                    // if there is no manual link, but the image contains an exact location use it.
                    var location = FindStuff.LocationsFinder.FindForFile(slideEntry.ImageFile, verbose);

                    if (location != null)
                    {
                        slideEntry.Link = $"https://earth.google.com/web/search/{location.Lat.ToString().Replace(",", ".")},{location.Lon.ToString().Replace(",", ".")}";
                    }
                }

                if (!string.IsNullOrWhiteSpace(slideEntry.Link))
                {
                    sb.AppendLine($"<div class=\"top-right\" style=\"display: block; margin-top: 100px; margin-right: 20px;\">");

                    sb.AppendLine($"       <a href=\"{slideEntry.Link}\" target=\"_earth\">🌎</a>");
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");
            }
            
            return sb.ToString();
        }

        private static List<SlideEntry> GenerateSlideEntries(FileInfo[] files, DirectoryInfo outpuFilesDirectory)
        {
            List<SlideEntry> slideEntries = new List<SlideEntry>();

            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(outpuFilesDirectory.FullName, file.Name));

                string quote = "";
                if (File.Exists(file.FullName + ".txt"))
                {
                    quote = File.ReadAllText(file.FullName + ".txt").Replace("  ", "</br>");
                }

                string titel = "";
                if (File.Exists(file.FullName + ".titel.txt"))
                {
                    titel = File.ReadAllText(file.FullName + ".titel.txt");
                }

                string link = "";
                if (File.Exists(file.FullName + ".link.txt"))
                {
                    link = File.ReadAllText(file.FullName + ".link.txt");
                }

                slideEntries.Add(new SlideEntry()
                {
                    ImageFile = file,
                    Quote = quote,
                    Titel = titel,
                    Link = link,
                    Date = file.CreationTime,
                });
            }

            return slideEntries;
        }
    }

    public class SlideEntry
    {
        public FileInfo ImageFile { get; set; }
        public string Quote { get; set; }
        public string Titel { get; set; }
        public string Link { get; set; }
        public DateTime Date { get; set; }
    }

}

