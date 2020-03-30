using System;
using System.Collections.Generic;

namespace OpenVIII.Search
{
    /// <summary>
    /// Application searches libraries for strings. Not the fastest thing.
    /// </summary>
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Memory.Init(null, null, null, args);
            Console.Write("Insert string you would like to find. This assumes you are looking for FF8 formatted strings." +
                "\r\nThere may be special characters in your string so you might have to remove words to get a match.\r\n The search is case sensitive.\r\n Input:  ");
            FF8String sval = Console.ReadLine()?.Trim((Environment.NewLine + " _").ToCharArray());
            if (sval == null) return;
            //new FF8String(new byte[] { 0x10, 0, 0, 0, 0x02, 0, 0, 0 });
            var s = new Search(sval);//used to find file a string is in. disable if not using.
            var rs = s.Results;
            Console.WriteLine(rs.Count > 0
                ? $"Found \"{sval}\" {rs.Count} times. Results below."
                : $"Cannot find \"{sval}\"...");
            foreach (var r in rs)
            {
                Console.WriteLine($"{r.Item1}, {r.Item2}, {r.Item3}");
            }
            Console.WriteLine($"Press [Enter] to exit...");
            Console.ReadLine();
        }

        #endregion Methods
    }
}