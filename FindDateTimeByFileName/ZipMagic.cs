using ICSharpCode.SharpZipLib.Zip;

namespace FindStuff
{
    public static class ZipMagic
    {
        internal static List<ZipEntry> ZipEntries = new List<ZipEntry>();

        internal static ZipFile Zipfile { get; set; }

        static ZipMagic()
        {
            var zip = new ZipInputStream(File.OpenRead(@"dates.zip"));
            var filestream = new FileStream(@"dates.zip", FileMode.Open, FileAccess.Read);
            Zipfile = new ZipFile(filestream);
            ZipEntry item;
            while ((item = zip.GetNextEntry()) != null)
            {
                ZipEntries.Add(item);
            }
        }
    }

   
}
