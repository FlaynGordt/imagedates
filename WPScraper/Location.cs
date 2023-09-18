using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WPScraper
{
    public class Location
    {
        public DateTime Date { get; set; }
        public float Lon { get; set; }
        public float Lat { get; set; }

        public static List<Location> Locations = new List<Location>();

        public static List<Location> Parse(string fileName)
        {
            JObject o = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(fileName));

            foreach (JObject element in o.First.Values())
            {
                var time = element.Value<long>("time");
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(time).DateTime;

                Locations.Add(new Location { Date = dateTime, Lon = element.Value<float>("lon"), Lat = element.Value<float>("lat") });
            }

            Locations = Locations.OrderBy(l => l.Date).ToList();

            return Locations;
        }

        internal static List<Location> FindLocationsForDates(List<DateTime> dates)
        {
            var result = new List<Location>();

            foreach (var date in dates) 
            {
                result.AddRange(Locations.Where(l => l.Date.Date == date.Date));
            }


            return result;
        }
    }

}
