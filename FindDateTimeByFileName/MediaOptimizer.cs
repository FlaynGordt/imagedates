using System.Diagnostics;

namespace FindStuff
{
    public class MediaOptimizer
    {
        public static void OptimizeJpg(string path)
        {
            var allFiles = Directory.EnumerateFiles(path, "*.jpg").Select(f => new FileInfo(f));

            long savedTotal = 0;

            foreach (var file in allFiles)
            {
                var oldFileCreateTime = file.CreationTime;
                long startLength = file.Length;

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;

                    p.StartInfo.FileName = Paths.OPTIMIZT_CMD;
                    p.StartInfo.Arguments = file.FullName;
                    p.Start();
                    p.WaitForExit();
                }

                var newFileInfo = new FileInfo(file.FullName);
                long endLength = newFileInfo.Length;

                savedTotal += (startLength - endLength);

                newFileInfo.CreationTime = oldFileCreateTime;
            }

            Console.WriteLine("Totally saved bytes: " + savedTotal.ToString());
        }

        public static void Mp42Webm(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.mp4").Select(f => new FileInfo(f)).ToArray();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo? mp4File = files[i];
                var webmFileName = mp4File.FullName.Replace(".mp4", ".webm");

                if (File.Exists(webmFileName))
                {
                    if (new FileInfo(webmFileName).Length > 0)
                    {
                        var webmFileExistsing = new FileInfo(webmFileName);
                        webmFileExistsing.CreationTime = mp4File.CreationTime;

                        // already converted but source not moved
                        mp4File.MoveTo(Path.Combine(FindStuff.Paths.BACKUP, mp4File.Name), true);
                        continue;
                    }
                    else
                    {
                        // convert was aborted, retry
                        File.Delete(webmFileName);
                    }
                }

                Console.WriteLine(i.ToString() + "/" + files.Length + " " + mp4File.Name);

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = FindStuff.Paths.FFMPEG_EXE;
                    p.StartInfo.Arguments = $"-v quiet -stats -i \"{mp4File.FullName}\" \"{webmFileName}\" ";
                    p.Start();
                    p.WaitForExit();
                }

                var webmFile = new FileInfo(webmFileName);
                webmFile.CreationTime = mp4File.CreationTime;
                mp4File.MoveTo(Path.Combine(FindStuff.Paths.BACKUP, mp4File.Name), true);

                Console.WriteLine(i.ToString() + "/" + webmFile.Length + " " + webmFile.Name);
            }
        }
    }


}
