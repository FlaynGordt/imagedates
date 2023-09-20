using FindStuff;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GenerateDocxII
{
    public class Post
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public XElement Content { get; set; }

        public List<LocationWithDate> Locations { get; set; }

        public List<DateTime> Dates { get; set; }

        public static List<DateTime> ParseDatesFromTitel(string title)
        {
            var fixedTitle = Regex.Replace(title, "[^0-9.]", " "); ;

            List<DateTime> list = new List<System.DateTime>();

            if (title.Contains("ein holpriger Start")) list.Add(DateTime.Parse("01.07.2022"));
            if (title.Contains("endlich in Ecuador!")) list.Add(DateTime.Parse("02.07.2022"));
            if (title.Contains("Tag 8:")) list.Add(DateTime.Parse("08.07.2022"));
            if (title.Contains("Tag 8:")) list.Add(DateTime.Parse("09.07.2022"));
            if (title.Contains("22.13")) list.Add(DateTime.Parse("22.12.2022"));
            if (title.Contains("29.-31.06")) list.Add(DateTime.Parse("29.06.2023"));
            if (title.Contains("29.-31.06")) list.Add(DateTime.Parse("30.06.2023"));

            foreach (var elems in fixedTitle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length > 1))
            {
                foreach (var elem in elems.Split('-'))
                {
                    try
                    {
                        var temp = elem;

                        if (temp.EndsWith("."))
                        {
                            temp = temp.Remove(temp.Length - 1, 1);
                        }
                        var foundate = DateTime.Parse(temp);

                        foundate = foundate.AddYears(-1);

                        if (foundate.Month < 7)
                        {
                            foundate = foundate.AddYears(1);
                        }

                        if (title.Contains("Klotok")
                            || title.Contains("Jakarta")
                            || title.Contains("Bali")
                            || title.Contains("Claudia hat schon wieder")
                            || title.Contains("Das Ende")
                            || title.Contains("Willkommensparty")
                            )
                        {
                            foundate = foundate.AddYears(1);
                        }

                        list.Add(foundate);
                    }
                    catch (Exception ex)
                    {
                        //ex.Message;
                    }
                }
            }

            //if(list.Count == 0)
            //{
            //	$"{title}".Dump();
            //}

            //$"{title} -> {string.Join(",", list)}";

            return list;
        }

    }
}
