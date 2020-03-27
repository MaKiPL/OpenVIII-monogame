using System.Text;

namespace OpenVIII.Fields.Scripts
{
    public sealed class ScriptWriter
    {
        #region Fields

        private readonly StringBuilder _sb;
        private bool _newLine;

        #endregion Fields

        #region Constructors

        public ScriptWriter(int capacity = 8096) => _sb = new StringBuilder(capacity);

        #endregion Constructors

        #region Properties

        public bool HasWhiteLine { get; set; }
        public int Indent { get; set; }

        #endregion Properties

        #region Methods

        public void Append(string text)
        {
            AppendIndent();

            _sb.Append(text);
        }

        public void AppendLine()
        {
            if (_newLine)
                HasWhiteLine = true;

            _sb.AppendLine();
            _newLine = true;
        }

        public void AppendLine(string text)
        {
            AppendIndent();

            _sb.AppendLine(text);
            _newLine = true;
            HasWhiteLine = (text == "{");
        }

        public string Release()
        {
            var result = _sb.ToString();

            _sb.Clear();
            Indent = 0;
            _newLine = true;
            HasWhiteLine = false;

            return result;
        }

        public State RememberState() => new State(this);

        public override string ToString() => _sb.ToString();

        private void AppendIndent()
        {
            if (_newLine)
            {
                for (var i = 0; i < Indent; i++)
                    _sb.Append("    ");

                _newLine = false;
                HasWhiteLine = false;
            }
        }

        #endregion Methods

        #region Classes

        public sealed class State
        {
            #region Fields

            private readonly ScriptWriter _sw;

            private bool _hasEmptyLine;
            private int _indent;
            private int _length;
            private bool _newLine;

            #endregion Fields

            #region Constructors

            public State(ScriptWriter sw)
            {
                _sw = sw;

                _indent = _sw.Indent;
                _newLine = _sw._newLine;
                _length = _sw._sb.Length;
                _hasEmptyLine = _sw.HasWhiteLine;
            }

            #endregion Constructors

            #region Properties

            public bool IsChanged => _length != _sw._sb.Length;

            #endregion Properties

            #region Methods

            public void Cancel()
            {
                _sw.Indent = _indent;
                _sw._newLine = _newLine;
                _sw.HasWhiteLine = _hasEmptyLine;
                _sw._sb.Length = _length;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}