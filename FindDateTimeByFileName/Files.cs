namespace FindStuff
{
    public static class Files
    {
        public static List<FileInfo> GetFiles(string path, SearchOption searchOption= SearchOption.TopDirectoryOnly)
        {
            string[] extensions = new string[] { "jpg", "webm", "png" };

            List<FileInfo> relevantFiles = new List<FileInfo>();

            foreach (var extension in extensions)
            {
                relevantFiles = relevantFiles.Concat(Directory.GetFiles(path, "*." + extension, searchOption).Select(f => new FileInfo(f))).ToList();
            }

            return relevantFiles;
        }
    }

   
}
