using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Attempt at a log class.
    /// </summary>
    internal class Log : TextWriter
    {
        private FileStream fs;
        private StreamWriter log;
        private bool enabled = false;

        public Log()
        {
            fs = new FileStream("log.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            log = new StreamWriter(fs, System.Text.Encoding.UTF8)
            { AutoFlush = true };
        }

        public override System.Text.Encoding Encoding => log.Encoding;
        /// <summary>
        /// If Disabled the log.txt will be empty. and Async writes will be null.
        /// </summary>
        public bool Enabled { get => enabled; set => enabled = value; }

        public override void Write(char value)
        {
            if (Enabled) log.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (Enabled) log.Write(buffer, index, count);
        }

        public override void Write(string value)
        {
            if (Enabled) log.Write(value);
        }

        public override void Write(char[] buffer)
        {
            if (Enabled) log.Write(buffer);
        }

        [ComVisible(false)]
        public override Task WriteAsync(char value) => !Enabled ? null : log.WriteAsync(value);

        [ComVisible(false)]
        public override Task WriteAsync(string value) => !Enabled ? null : log.WriteAsync(value);

        [ComVisible(false)]
        public override Task WriteAsync(char[] buffer, int index, int count) => !Enabled ? null : log.WriteAsync(buffer, index, count);

        [ComVisible(false)]
        public override Task WriteLineAsync() => !Enabled ? null : log.WriteLineAsync();

        [ComVisible(false)]
        public override Task WriteLineAsync(char value) => !Enabled ? null : log.WriteLineAsync(value);

        [ComVisible(false)]
        public override Task WriteLineAsync(string value) => !Enabled ? null : log.WriteLineAsync(value);

        [ComVisible(false)]
        public override Task WriteLineAsync(char[] buffer, int index, int count) => !Enabled ? null : log.WriteLineAsync(buffer, index, count);

        [ComVisible(false)]
        public override void Close() => log.Close();

        [ComVisible(false)]
        public override void Flush() => log.Flush();

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