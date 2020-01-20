using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Attempt at a log class.
    /// </summary>
    internal class Log : TextWriter
    {
        FileStream fs;
        StreamWriter log;

        public Log()
        {
            fs = new FileStream("log.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            log = new StreamWriter(fs, System.Text.Encoding.UTF8)
            { AutoFlush = true };
        }

        public override System.Text.Encoding Encoding => log.Encoding;
        public override void Write(char value) => log.Write(value);
        public override void Write(char[] buffer, int index, int count) => log.Write(buffer, index, count);
        public override void Write(string value) => log.Write(value);
        public override void Write(char[] buffer) => log.Write(buffer);


        [ComVisible(false)]
        public override Task WriteAsync(char value) => log.WriteAsync(value);
        [ComVisible(false)]
        public override Task WriteAsync(string value) => log.WriteAsync(value);
        [ComVisible(false)]
        public override Task WriteAsync(char[] buffer, int index, int count) => log.WriteAsync(buffer, index, count);
        [ComVisible(false)]
        public override Task WriteLineAsync() => log.WriteLineAsync();
        [ComVisible(false)]
        public override Task WriteLineAsync(char value) => log.WriteLineAsync(value);
        [ComVisible(false)]
        public override Task WriteLineAsync(string value) => log.WriteLineAsync(value);
        [ComVisible(false)]
        public override Task WriteLineAsync(char[] buffer, int index, int count) => log.WriteLineAsync(buffer, index, count);

        [ComVisible(false)]
        public override void Close() => log.Close();

        [ComVisible(false)]
        public override void Flush() => log.Close();
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //log?.Close();
            //log?.Dispose();
            log = null;
            fs?.Close();
            fs?.Dispose();
            fs = null;
        }
        ~Log()
        {
            Dispose(true);
        }
    }
}
