using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FindStuff
{
    public class Location
    {
        public float Lat { get; set; }
        public float Lon { get; set; }
    }

    public class LocationWithDate : Location
    {
        public DateTime Date { get; set; }
    }
}
