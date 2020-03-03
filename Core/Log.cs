using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Attempt at a log class.
    /// </summary>
    public class Log : TextWriter
    {
        #region Fields

        private FileStream _fs;
        private StreamWriter _log;

        #endregion Fields

        #region Constructors

        public Log()
        {
            _fs = new FileStream("log.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _log = new StreamWriter(_fs, System.Text.Encoding.UTF8)
            { AutoFlush = true };
        }

        #endregion Constructors

        #region Destructors

        ~Log()
        {
            Dispose(true);
        }

        #endregion Destructors

        #region Properties

        /// <summary>
        /// If Disabled the log.txt will be empty. and Async writes will be null.
        /// </summary>
        public bool Enabled { get; set; } = false;

        public override System.Text.Encoding Encoding => _log.Encoding;

        #endregion Properties

        #region Methods

        [ComVisible(false)]
        public override void Close() => _log.Close();

        [ComVisible(false)]
        public override void Flush() => _log.Flush();

        public override void Write(char value)
        {
            if (Enabled) _log.Write(value);
            else Debug.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (Enabled) _log.Write(buffer, index, count);
            else Debug.Write(new string(buffer.Skip(index).Take(count).ToArray()));
        }

        public override void Write(string value)
        {
            if (Enabled) _log.Write(value);
            else Debug.Write(value);
        }

        public override void Write(char[] buffer)
        {
            if (Enabled) _log.Write(buffer);
            else Debug.Write(buffer);
        }

        [ComVisible(false)]
        public override Task WriteAsync(char value) => !Enabled ? null : _log.WriteAsync(value);

        [ComVisible(false)]
        public override Task WriteAsync(string value) => !Enabled ? null : _log.WriteAsync(value);

        [ComVisible(false)]
        public override Task WriteAsync(char[] buffer, int index, int count) => !Enabled ? null : _log.WriteAsync(buffer, index, count);

        public override void WriteLine(string value) => base.WriteLine($"{Thread.CurrentThread.ManagedThreadId}:{Task.CurrentId}:{value}");

        [ComVisible(false)]
        public override Task WriteLineAsync() => !Enabled ? null : _log.WriteLineAsync();

        [ComVisible(false)]
        public override Task WriteLineAsync(char value) => !Enabled ? null : _log.WriteLineAsync(value);

        [ComVisible(false)]
        public override Task WriteLineAsync(string value) => !Enabled ? null : _log.WriteLineAsync(value);

        [ComVisible(false)]
        public override Task WriteLineAsync(char[] buffer, int index, int count) => !Enabled ? null : _log.WriteLineAsync(buffer, index, count);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //log?.Close();
            //log?.Dispose();
            _log = null;
            _fs?.Close();
            _fs?.Dispose();
            _fs = null;
        }

        #endregion Methods
    }
}