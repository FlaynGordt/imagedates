using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LocationStuff
{
    public static class Locations
    {
        private static string FILENAME = @"C:\privat\sqlite\locations.json";

        private static Lazy<List<Location>> Cache = new Lazy<List<Location>>(() => {
            JObject o = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(FILENAME));

            var res = new List<Location>();

            foreach (JObject element in o.First.Values())
            {
                var time = element.Value<long>("time");
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime;

                var newLocation = new Location { Date = dateTime, Lon = element.Value<float>("lon"), Lat = element.Value<float>("lat") };


                res.Add(newLocation);
            }

            return res;
        });

        public static List<Location> FindByDateNotTime(DateTime date)
        {
            var res = new Dictionary<System.DateTime, List<Location>>();

            foreach (var location in Cache.Value)
            {
                List<Location> list;
                if (!res.TryGetValue(location.Date.Date, out list))
                {
                    list = new List<Location>();
                    res.Add(location.Date.Date, list);
                }
                list.Add(location);
            }

            if (res.TryGetValue(date.Date, out var result))
            {
                return result;
            }

            return new List<Location>();
        }

        public static Location FindClosestToDateTime(DateTime dateTime)
        {
            Location result = null;
            
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

        public static List<Location> DouglasPeuckerSimplify(List<Location> points, double epsilon)
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

            List<Location> result = new List<Location>();

            if (dMax > epsilon)
            {
                List<Location> recursiveResults1 = DouglasPeuckerSimplify(points.GetRange(0, index + 1), epsilon);
                List<Location> recursiveResults2 = DouglasPeuckerSimplify(points.GetRange(index, points.Count - index), epsilon);

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

        public static double PerpendicularDistance(Location point, Location lineStart, Location lineEnd)
        {
            double area = Math.Abs(0.5 * (lineStart.Lon * lineEnd.Lat + lineEnd.Lon * point.Lat + point.Lon * lineStart.Lat -
                                 lineEnd.Lon * lineStart.Lat - point.Lon * lineEnd.Lat - lineStart.Lon * point.Lat));
            double bottom = Math.Sqrt(Math.Pow(lineEnd.Lon - lineStart.Lon, 2) + Math.Pow(lineEnd.Lat - lineStart.Lat, 2));
            double height = area / bottom * 2;

            return height;
        }

       
    }
}