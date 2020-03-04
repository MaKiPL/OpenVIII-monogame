using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.PAK_Extractor
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            
            Memory.Init(null, null, null, null);

            string DestPath = Path.Combine(Memory.FF8DIRdata, "movies");
//DestPath = Path.Combine(Path.GetTempPath(), "movies"); //comment out to default to ff8 folder.
start:
            List<FileInfo> files = new List<FileInfo>(1);
            List<PAK> data = new List<PAK>(1);
            Console.WriteLine($"This tool is used to extract the movies from the FF8 CDs, to be more like the steam version.");
            Console.WriteLine($"Detecting Compact Disc drives...");
            foreach (DriveInfo drive in DriveInfo.GetDrives()
                               .Where(d => d.DriveType == DriveType.CDRom && d.IsReady))
            {
                DirectoryInfo dir = drive.RootDirectory;

                Console.WriteLine($"Found disc: {dir}");

                Console.WriteLine($"Scanning for PAK files...");
                files.AddRange(dir.GetFiles("*", SearchOption.TopDirectoryOnly).Where(x =>
                  x.FullName.EndsWith(".pak", StringComparison.OrdinalIgnoreCase)));
            }
            foreach (FileInfo f in files)
            {
                Console.WriteLine($"PAK file detected: {f}");
                PAK pak = new PAK(f);
                long SumHI = pak.Movies.Sum(g => g.BINK_HIGH.Size);
                long SumLO = pak.Movies.Sum(g => g.BINK_LOW.Size);
                long SumCA = pak.Movies.Sum(g => g.CAM.Size);
                Console.WriteLine($"PAK has {pak.Count} videos. Total of {SumHI / 1024} KB (HI Res BINK), {SumLO / 1024} KB (LOW Res BINK), {SumCA / 1024} KB (CAM)");
                data.Add(pak);
            }

            if (data.Count > 0)
            {
                Console.WriteLine($"Destination: {Path.GetFullPath(DestPath)}\nPress [Enter] to continue. If you'd like to override this path, type a new destination, and then press [ENTER].");
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input.Trim()))
                {
                    Console.WriteLine($"Changed destination to: {Path.GetFullPath(input)}");
                    DestPath = input;
                }
                if (!Directory.Exists(DestPath))
                {
                    Console.WriteLine($"Created directory.");
                    Directory.CreateDirectory(DestPath);
                }
                foreach (PAK pak in data)
                {
                    pak.Extract(DestPath);
                }
            }

            Console.WriteLine($"Insert disc and press [Enter].");
            Console.ReadLine();
            goto start;
        }

        #endregion Methods
    }
}