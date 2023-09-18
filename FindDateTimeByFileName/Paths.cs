namespace FindStuff
{
    public static class Paths
    {
        public static string SLIDYROOT = @"C:\privat\git\slidy\";

        public static string BTD_PATH = Path.Combine(SLIDYROOT, "BDT");
        public static string OUT_PATH = Path.Combine(SLIDYROOT, "out");
        public static string HTML_FILES_PATH = Path.Combine(SLIDYROOT,"..", "imagedates", "HTML Slidy_files");
        public static string HTML_TEMPATE_PATH = Path.Combine(HTML_FILES_PATH, "HTML Slidy.html");
        public static string BACKUP = Path.Combine(SLIDYROOT, "backup");
        public static string FFMPEG_EXE = Path.Combine(SLIDYROOT, "ffmpeg","ffmpeg.exe");
    }


}
