using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Search
{
    class Program
    {
        static void Main(string[] args)
        {
            start:
            Memory.Init(null, null, null, null);
            Console.Write("Insert string you would like to find. This assumes you are looking for FF8 formated strings." +
                "\r\nThere may be special characters in your string so you might have to remove words to get a match.\r\n The search is case senseative.\r\n Input:  ");
            FF8String sval = Console.ReadLine().Trim((Environment.NewLine+" _").ToCharArray());
                //new FF8String(new byte[] { 0x10, 0, 0, 0, 0x02, 0, 0, 0 });
            Search s = new Search(sval);//used to find file a string is in. disable if not using.
            var rs = s.results;
            if (rs.Count > 0)
                Console.WriteLine($"Found \"{sval}\" {rs.Count} times. Results below.");
            else Console.WriteLine($"Cannot find \"{sval}\"...");
            foreach (var r in rs)
            {
                Console.WriteLine($"{r.Item1}, {r.Item2}, {r.Item3}");
            }
            Console.WriteLine($"Press [Enter] to continue...");
            Console.ReadLine();
            goto start;
        }
    }
}
