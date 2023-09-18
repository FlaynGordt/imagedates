using System.Collections.Generic;
using System;

namespace LocationStuff
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var closest = Locations.FindClosestToDateTime(new DateTime(2022, 07, 12, 14, 00, 00));
            var dawda = Locations.FindByDateNotTime(new DateTime(2022, 07, 12, 14, 00, 00));
        }
    }
}