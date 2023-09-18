using ExifLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FindStuff
{

    public static class DateFinder
    {
      

        private static Dictionary<string, DateTime> FallbackAndOverride = new Dictionary<string, DateTime>();

        static DateFinder()
        {

            FallbackAndOverride.Add("Point Blur_Aug_222022_185600", new DateTime(2022, 08, 22, 18, 56, 00));
            FallbackAndOverride.Add("DJI_0621", new DateTime(2022, 08, 27, 14, 26, 00));
            FallbackAndOverride.Add("DJI_0635", new DateTime(2022, 08, 29, 11, 22, 00));
        }

        static public DateTime? SearchDate(FileInfo file, bool verbose = false)
        {
            string fileNameWithoutExtension =file.Name.Substring(0, file.Name.Length - file.Extension.Length);

            if (verbose) Console.WriteLine("SearchDate: Searching for " + fileNameWithoutExtension + ":");
            DateTime? jsonDate = null;
            DateTime? filenameDate = null;
            DateTime? exifDate = null;

            string jsonResult = FindInJson(fileNameWithoutExtension, verbose);
            if (!string.IsNullOrEmpty(jsonResult))
            {
                var dto = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jsonResult));
                jsonDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jsonResult)).ToLocalTime().DateTime;

                //// Define the Chile Standard Time (CLT) time zone
                //TimeZoneInfo chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

                //// Convert the DateTimeOffset to the CLT time zone
                //jsonDate = TimeZoneInfo.ConvertTime(dto, chileTimeZone).DateTime;

            }

            var fileNameResult = GetByFileName(fileNameWithoutExtension, verbose);

            if (!string.IsNullOrEmpty(fileNameResult))
            {
                var number = long.Parse(fileNameResult);
                if (number < 253402300799) 
                {
                    filenameDate = DateTimeOffset.FromUnixTimeSeconds(number).DateTime;
                }
            }

            exifDate = FromExifData(file, verbose);

            if (exifDate.HasValue && filenameDate.HasValue||
                exifDate.HasValue && jsonDate.HasValue ||
                filenameDate.HasValue && jsonDate.HasValue)
            {
                

            }

            var date = jsonDate ?? filenameDate ?? exifDate;

            var withValue = 0;
            long totalTicks = 0;

            if (exifDate.HasValue)
            {
                totalTicks = totalTicks + exifDate.Value.Ticks;
                withValue++;
            }
            if (filenameDate.HasValue)
            {
                totalTicks = totalTicks + filenameDate.Value.Ticks;
                withValue++;
            }
            if (jsonDate.HasValue)
            {
                totalTicks = totalTicks + jsonDate.Value.Ticks;
                withValue++;
            }

            if ( withValue > 0 )
            {
                date = new DateTime(totalTicks / withValue);
            }


            if (FallbackAndOverride.TryGetValue(fileNameWithoutExtension, out var fallbackDate))
            {
                if (date.HasValue && verbose) Console.WriteLine("SearchDate: " + fileNameWithoutExtension + $" has date {date}, but was also found in Fallback/Override");
                else if (verbose) Console.WriteLine("SearchDate: " + fileNameWithoutExtension + " found in Fallback/Override");

                date = fallbackDate;
            }

            return date;
        }

        private static DateTime? FromExifData(FileInfo file, bool verbose)
        {
            if (!file.Exists) { return null; }

            try
            {
                using (ExifReader reader = new ExifReader(file.FullName))
                {
                    DateTime dateTaken;
                    if (reader.GetTagValue(ExifTags.DateTimeOriginal, out dateTaken))
                    {
                        if (verbose) Console.WriteLine("Datefinder.FromExifData: " + "Found in exif data");
                        return dateTaken;
                    }
                }
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("Datefinder.FromExifData: " + $"Error: {ex.Message}");
            }

            return null;
        }

        private static string GetByFileName(string fileName, bool verbose)
        {
            if (fileName.Contains("PXL"))
            {
                fileName = fileName.Split(new[] { "PXL" }, StringSplitOptions.None)[1];
            }

            if (fileName.Contains("IMG"))
            {
                fileName = fileName.Split(new[] { "IMG" }, StringSplitOptions.None)[1];
                fileName = fileName.Replace("-", "_");

                if (fileName.StartsWith("_"))
                {
                    fileName = fileName.Substring(1, fileName.Length - 1);
                }
            }

            if (verbose) Console.WriteLine("Datefinder.GetByFileName: " + $"Looking dates in {fileName}");

            var splits = fileName.Split('.')[0].Split('~')[0].Split('-')[0].Split('_').ToList();

            var dateEntry = splits.FirstOrDefault(s => s.StartsWith("202"));
            if (dateEntry == null)
            {
                // unix timestamp
                var unixTimeStamp = splits.FirstOrDefault(s => s.StartsWith("16"));

                if (unixTimeStamp != null && long.TryParse(unixTimeStamp, out var _))
                {
                    return unixTimeStamp;
                }

                return null;
            }

            if (verbose) Console.WriteLine("Datefinder.GetByFileName: " + $"No unix timestamp in {fileName}");

            var indexOfDate = splits.IndexOf(dateEntry);

            var timeEntry = splits[indexOfDate + 1].Substring(0, 6);
            DateTime date;
            try
            {
                date = DateTime.ParseExact(dateEntry, "yyyyMMdd", null);

                if (TimeSpan.TryParseExact(timeEntry, "hhmmss", null, out var time))
                {
                    date = date.Add(time);
                }

                if (verbose) Console.WriteLine("Datefinder.GetByFileName: " + $"Found yyyyMMdd in {fileName}");
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("Datefinder.GetByFileName: " + ex.Message);
                return null;
            }

            if (verbose) Console.WriteLine();
            
            DateTimeOffset dateTimeOffset = TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.Utc);
            return dateTimeOffset.ToUnixTimeSeconds().ToString();
        }

        private static string FindInJson(string fileName, bool verbose)
        {
            if (fileName.StartsWith("original_"))
            {
                fileName = fileName.Split('_').Skip(1).First();
            }

            foreach (var item in ZipMagic.ZipEntries)
            {
                // find by filename
                if (item.Name.ToLower().Contains(fileName.ToLower()))
                {
                    using (StreamReader s = new StreamReader(ZipMagic.Zipfile.GetInputStream(item)))
                    {
                        JToken tokens = (JToken)JsonConvert.DeserializeObject(s.ReadToEnd());

                        var date = (string)tokens.SelectToken("photoTakenTime.timestamp");
                        if (verbose) Console.WriteLine("DateFinder.FindInJson: " + fileName + " found in zip. Original name: " + item.Name);
                        return date;
                    }
                }
            }

            if (verbose) Console.WriteLine("DateFinder.FindInJson: " + fileName + " not found in zip."); 

            return null;
        }
    }

   
}
