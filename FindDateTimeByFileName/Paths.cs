namespace FindStuff
{
    public static class Paths
    {
        // w
        public static string SLIDY_ROOT = @"C:\privat\git\slidy\";
        public static string FFMPEG_EXE = @"C:\privat\ffmpeg\ffmpeg.exe";
        public static string OPTIMIZT_CMD = @"C:\Users\Florian.Gerhardt\AppData\Roaming\npm\optimizt.cmd";

        // p
        //public static string SLIDY_ROOT = @"C:\privat\git\slidy\";
        //public static string FFMPEG_EXE = Path.Combine(SLIDY_ROOT, "ffmpeg", "ffmpeg.exe");

        public static string BTD = Path.Combine(SLIDY_ROOT, "BDT");
        public static string SLIDY_OUT = Path.Combine(SLIDY_ROOT, "out");
        public static string HTML_FILES = Path.Combine(SLIDY_ROOT,"..", "imagedates", "HTML Slidy_files");
        public static string HTML_TEMPATE = Path.Combine(HTML_FILES, "HTML Slidy.html");
        public static string BACKUP = Path.Combine(SLIDY_ROOT, "backup");
      
        public static string DOCX_OUT = SLIDY_ROOT;

        public static string Combine(this string path, Country country)
        {
            return Path.Combine(path, country.ToString());
        }
    }
}
