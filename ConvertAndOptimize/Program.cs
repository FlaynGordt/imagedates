using System.Diagnostics;

namespace ConvertAndOptimize
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var allFiles = Directory.EnumerateFiles(@"C:\privat\slidy\BDT", "*.jpg").Select(f => new FileInfo(f));

            //foreach (var file in allFiles)
            //{
            //    //Console.Write(file.FullName + " " + file.Length);
            //    using (Process p = new Process())
            //    {
            //        p.StartInfo.UseShellExecute = false;

            //        p.StartInfo.FileName = @"C:\Users\Florian.Gerhardt\AppData\Roaming\npm\optimizt.cmd";
            //        p.StartInfo.Arguments = file.FullName;
            //        p.Start();
            //        p.WaitForExit();
            //    }

            //    //Console.WriteLine("->" + new FileInfo(file.FullName).Length);
            //}

            Mp42Webm();

            Console.WriteLine("done");
            Console.ReadLine();
        }

          public static void Mp42Webm()
    {
        var files = Directory.EnumerateFiles(FindStuff.Paths.BTD_PATH, "*.mp4").Select(f => new FileInfo(f)).OrderBy(f => f.CreationTime).ToArray();

        foreach (var file in files)
        {
            var newFileName = file.FullName.Replace(".mp4", ".webm");

            if (File.Exists(newFileName))
            {
                new FileInfo(newFileName).CreationTime = file.CreationTime;
                Console.WriteLine("skipping " + newFileName);

                file.MoveTo(Path.Combine(FindStuff.Paths.BACKUP, file.Name));
                continue;
            }

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = FindStuff.Paths.FFMPEG_EXE;;
                p.StartInfo.Arguments = $"-v quiet -stats -i \"{file.FullName}\" \"{newFileName}\" ";
                p.Start();
                p.WaitForExit();

                new FileInfo(newFileName).CreationTime = file.CreationTime;

                file.MoveTo(Path.Combine(FindStuff.Paths.BACKUP, file.Name));
            }
        }
    }

    }

  }