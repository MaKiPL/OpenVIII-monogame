using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Extract Bink Movies from CDs
/// </summary>
namespace OpenVIII.PAK_Extractor
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Memory.Init(null, null, null, args);

            var destPath = Path.Combine(Memory.FF8DirData, "movies");
            //DestPath = Path.Combine(Path.GetTempPath(), "movies"); //comment out to default to ff8 folder.

            var files = new List<FileInfo>(1);
            var data = new List<PAK>(1);
            Console.WriteLine("This tool is used to extract the movies from the FF8 CDs, to be more like the steam version.");
            Console.WriteLine("Detecting Compact Disc drives...");
            foreach (var drive in DriveInfo.GetDrives()
                               .Where(d => d.DriveType == DriveType.CDRom && d.IsReady))
            {
                var dir = drive.RootDirectory;

                Console.WriteLine($"Found disc: {dir}");

                Console.WriteLine("Scanning for PAK files...");
                files.AddRange(dir.GetFiles("*", SearchOption.TopDirectoryOnly).Where(x =>
                  x.FullName.EndsWith(".pak", StringComparison.OrdinalIgnoreCase)));
            }
            foreach (var f in files)
            {
                Console.WriteLine($"PAK file detected: {f}");
                var pak = new PAK(f);
                var sumHigh = pak.Movies.Sum(g => g.BinkHigh.Size);
                var sumLow = pak.Movies.Sum(g => g.BinkLow.Size);
                var sumCam = pak.Movies.Sum(g => g.Cam.Size);
                Console.WriteLine($"PAK has {pak.Count} videos. Total of {sumHigh / 1024} KB (HI Res BINK), {sumLow / 1024} KB (LOW Res BINK), {sumCam / 1024} KB (CAM)");
                data.Add(pak);
            }

            if (data.Count > 0)
            {
                Console.WriteLine($"Destination: {Path.GetFullPath(destPath)}\nPress [Enter] to continue. If you'd like to override this path, type a new destination, and then press [ENTER].");
                var input = Console.ReadLine();
                if (input != null && !string.IsNullOrWhiteSpace(input.Trim()))
                {
                    Console.WriteLine($"Changed destination to: {Path.GetFullPath(input)}");
                    destPath = input;
                }
                if (!Directory.Exists(destPath))
                {
                    Console.WriteLine("Created directory.");
                    Directory.CreateDirectory(destPath);
                }
                foreach (var pak in data)
                {
                    pak.Extract(destPath);
                }
            }

            Console.WriteLine("Insert disc and press [Enter].");
            Console.ReadLine();
        }

        #endregion Methods
    }
}