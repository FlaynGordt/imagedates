using Newtonsoft.Json;

namespace FindStuff
{
    public static class Extensions
    {
        public static string Combine(this string path, Country country)
        {
            return Path.Combine(path, country.ToString());
        }
        public static FileMeta Load(this FileInfo file)
        {
            var metaFileName = file.FullName + ".json";

            if (File.Exists(metaFileName))
            {
                var fm =  JsonConvert.DeserializeObject<FileMeta>(File.ReadAllText(metaFileName));
                fm.OriginalFile = file;
                return fm;
            }

            var fmnew = new FileMeta() { DateTime = file.CreationTime, OriginalFile = file };
            fmnew.Save();
            return fmnew;
        }
    }
}
