using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ExifLib;

namespace FindStuff
{
    public static class LocationsFinder
    {
        private static string FILENAME = @"locations.json";

        private static Lazy<List<LocationWithDate>> Cache = new Lazy<List<LocationWithDate>>(() => {
            JObject o = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(FILENAME));

            var res = new List<LocationWithDate>();

            foreach (JObject element in o.First.Values())
            {
                var time = element.Value<long>("time");
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime;

                var newLocation = new LocationWithDate { Date = dateTime, Lon = element.Value<float>("lon"), Lat = element.Value<float>("lat") };


                res.Add(newLocation);
            }

            return res;
        });

        public static List<LocationWithDate> Locations
        {
            get { return Cache.Value; }
        }

        public static List<LocationWithDate> FindByDateNotTime(DateTime date)
        {
            var res = new Dictionary<DateTime, List<LocationWithDate>>();

            foreach (var location in Cache.Value)
            {
                List<LocationWithDate> list;
                if (!res.TryGetValue(location.Date.Date, out list))
                {
                    list = new List<LocationWithDate>();
                    res.Add(location.Date.Date, list);
                }
                list.Add(location);
            }

            if (res.TryGetValue(date.Date, out var result))
            {
                return result;
            }

            return new List<LocationWithDate>();
        }

        private static Location FindInJson(string fileName, bool verbose)
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

                        var latitude = (float)tokens.SelectToken("geoData.latitude");
                        var longitude = (float)tokens.SelectToken("geoData.longitude");
                        var latitudeExif = (float)tokens.SelectToken("geoDataExif.latitude");
                        var longitudeExif = (float)tokens.SelectToken("geoDataExif.longitude");

                        if (latitude == 0) latitude = latitudeExif;
                        if (longitude == 0) longitude = longitudeExif;

                        if (latitude == 0 || longitude == 0)
                        {
                            if (verbose) Console.WriteLine("LocationsFinder.FindInJson: " + fileName + " found in zip but values are 0");
                            continue;
                        }

                        if (verbose) Console.WriteLine("LocationsFinder.FindInJson: " + fileName + " found in zip: (" + latitude + ";" + longitude + ")");
                        return new Location() { Lat = latitude, Lon = longitude };
                    }
                }
            }

            if (verbose) Console.WriteLine("LocationsFinder.FindInJson: " + fileName + " not found in zip.");

            return null;
        }

        public static Location FindForFile(FileInfo file, bool verbose)
        {
            //try
            //{
            //    using (ExifReader reader = new ExifReader(file.FullName))
            //    {
            //        long lon, lat;
            //        if (reader.GetTagValue(ExifTags.GPSLongitude, out lon))
            //        {
            //            if (reader.GetTagValue(ExifTags.GPSLatitude, out lat))
            //            {
            //                return new Location() { Lat = lat, Lon = lon};
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    if (verbose) Console.WriteLine($"Error: {ex.Message}");
            //}

            return FindInJson(file.Name, verbose);
        }

        public static LocationWithDate FindClosestToDateTime(DateTime dateTime)
        {
            LocationWithDate result = null;

            foreach (var location in Cache.Value)
            {
                if (result == null)
                {
                    result = location;
                }
                else if (Math.Abs((location.Date - dateTime).Ticks) < Math.Abs((result.Date - dateTime).Ticks))
                {
                    result = location;
                }
            }

            return result;
        }

        public static List<LocationWithDate> DouglasPeuckerSimplify(List<LocationWithDate> points, double epsilon)
        {
            if (points.Count < 3)
                return points;

            int index = 0;
            double dMax = 0;

            for (int i = 1; i < points.Count - 1; i++)
            {
                double d = PerpendicularDistance(points[i], points[0], points[points.Count - 1]);
                if (d > dMax)
                {
                    index = i;
                    dMax = d;
                }
            }

            List<LocationWithDate> result = new List<LocationWithDate>();

            if (dMax > epsilon)
            {
                List<LocationWithDate> recursiveResults1 = DouglasPeuckerSimplify(points.GetRange(0, index + 1), epsilon);
                List<LocationWithDate> recursiveResults2 = DouglasPeuckerSimplify(points.GetRange(index, points.Count - index), epsilon);

                result.AddRange(recursiveResults1);
                result.AddRange(recursiveResults2);
            }
            else
            {
                result.Add(points[0]);
                result.Add(points[points.Count - 1]);
            }

            return result;
        }

        public static double PerpendicularDistance(LocationWithDate point, LocationWithDate lineStart, LocationWithDate lineEnd)
        {
            double area = Math.Abs(0.5 * (lineStart.Lon * lineEnd.Lat + lineEnd.Lon * point.Lat + point.Lon * lineStart.Lat -
                                 lineEnd.Lon * lineStart.Lat - point.Lon * lineEnd.Lat - lineStart.Lon * point.Lat));
            double bottom = Math.Sqrt(Math.Pow(lineEnd.Lon - lineStart.Lon, 2) + Math.Pow(lineEnd.Lat - lineStart.Lat, 2));
            double height = area / bottom * 2;

            return height;
        }


    }


}
