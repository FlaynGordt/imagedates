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
            
            Console.Title = country.ToString();

            GenerateSlides(country, true);
        }

        private static void GenerateSlides(Country country, bool verbose)
        {
            List<FileInfo> relevantFiles = Files.GetFiles(Paths.BTD.Combine(country));

            var files = relevantFiles.Select(f => f.Load())
                                     .Where(f => country != Country.Ecuador || CountryFilter(f.DateTime, country)) // für ecuador nach datum filtern
                                     .ToArray();

            DirectoryInfo outputDirectory = new DirectoryInfo(Paths.SLIDY_OUT.Combine(country));

            PrepareDirectoriesAndCopyFiles(outputDirectory);
            DirectoryInfo outputImageFilesDirectory = Directory.CreateDirectory(Path.Combine(outputDirectory.FullName, "files"));

            //List<SlideEntry> slideEntries = 
                CopyFiles(files, outputImageFilesDirectory);

            var generatedDivsText = GenerateHTMLDivs(files, verbose);

            var htmlTextGenerated = File.ReadAllText(FindStuff.Paths.HTML_TEMPATE);
            htmlTextGenerated = htmlTextGenerated.Replace("SLIDESPLACEHOLDER", generatedDivsText);
            htmlTextGenerated = htmlTextGenerated.Replace("COVERIMAGEPLACEHOLDER", country.ToString().ToLower() + ".jpg");
            htmlTextGenerated = htmlTextGenerated.Replace("TITELPLACEHOLDER", country.ToString().ToUpper());

            File.WriteAllText(Path.Combine(outputDirectory.FullName, "out.html"), htmlTextGenerated);

            Console.WriteLine(files.Length + " Slides. Press 'o' to open or any other key to close");
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

        private static string GenerateHTMLDivs(FileMeta[] slideEntries, bool verbose)
        {
            var sb = new StringBuilder();
            int counter = 0;
            foreach (var slideEntry in slideEntries.OrderBy(f => f.DateTime))
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine(++counter + " " + slideEntry.DateTime.ToString("ddMMyyyy") + " " + slideEntry.OriginalFile.Name);
                var relativeFileName = "files/" + slideEntry.OriginalFile.Name;

                sb.AppendLine("<div class=\"slide container hidden\">");
                sb.AppendLine($"  <h1>{slideEntry.Titel}")
                  .AppendLine($"</br>{slideEntry.OriginalFile.Name}-{slideEntry.OriginalFile.CreationTime.ToString("dd.MM.yyyy hh:mm")}")
                  .AppendLine("</h1> ");

                if (slideEntry.OriginalFile.FullName.ToLower().EndsWith("webm"))
                {
                    sb.AppendLine($"       <video src = \"{relativeFileName}\" controls autoplay muted loop lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }
                else if (slideEntry.OriginalFile.FullName.ToLower().EndsWith("mp4"))
                {
                    sb.AppendLine($"       <video src = \"{relativeFileName}\" controls autoplay muted loop lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }
                else
                {
                    sb.AppendLine($"       <img src=\"{relativeFileName}\" lazy height=\"85%\"  style=\"display: block; margin-left: auto; margin-right: auto;\"/>");
                }

                if (!string.IsNullOrWhiteSpace(slideEntry.Comment))
                {
                    sb.AppendLine($"<div class=\"bottom-right\">{slideEntry.Comment}</div>");
                }

                if (slideEntry.Pos != null)
                {
                    var link = $"https://earth.google.com/web/search/{slideEntry.Pos.Lat.ToString().Replace(",", ".")},{slideEntry.Pos.Lon.ToString().Replace(",", ".")}";

                    sb.AppendLine($"<div class=\"top-right\" style=\"display: block; margin-top: 100px; margin-right: 20px;\">");

                    sb.AppendLine($"       <a href=\"{link}\" target=\"_earth\">🌎</a>");
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");
            }
            
            return sb.ToString();
        }

        private static void CopyFiles(FileMeta[] files, DirectoryInfo outpuFilesDirectory)
        {
         
            foreach (var fileMeta in files.OrderBy(f => f.DateTime))
            {
                fileMeta.OriginalFile.CopyTo(Path.Combine(outpuFilesDirectory.FullName, fileMeta.OriginalFile.Name));

            }

        }

    }


}

