using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace FindStuff
{

    public class FileMeta
    {
        public string Titel { get; set; }
        public string Comment { get; set; }
        public Location Pos { get; set; }
        public DateTime DateTime { get; set; }

        [JsonIgnore]
        public FileInfo OriginalFile { get; set; }
     

        //public bool TryLoadFileMeta(string originalFileName, out FileMeta fileMeta)
        //{
        //    var metaFileName = originalFileName + ".json";

        //    if (File.Exists(metaFileName))
        //    {
        //        fileMeta =  JsonConvert.DeserializeObject<FileMeta>(File.ReadAllText(metaFileName));
        //        fileMeta.OriginalFile = new FileInfo(originalFileName);
        //        return true;
        //    }
        //    fileMeta = null;
        //    return false;
        //}

        public void Save()
        {
            var metaFileName = OriginalFile.FullName + ".json";
            File.WriteAllText(metaFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
