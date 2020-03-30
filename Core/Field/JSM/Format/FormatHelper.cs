using System;
using System.Globalization;

namespace OpenVIII.Fields.Scripts
{
    public static class FormatHelper
    {
        #region Fields

        private static readonly string[] Separators = { "\r\n", "\n", "{Line}", "{Next}" };

        #endregion Fields

        #region Methods

        public static Formatter Format(this ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext) => new Formatter(sw, formatterContext, executionContext);

        public static void FormatAnswers(ScriptWriter sw, string message, IJsmExpression top, IJsmExpression bottom, IJsmExpression begin, IJsmExpression cancel)
        {
            if (!(top is IConstExpression t) || !(bottom is IConstExpression b))
            {
                FormatMonologue(sw, message);
                return;
            }

            if (!sw.HasWhiteLine)
                sw.AppendLine();

            // Question
            //☞ Answer 1
            //   Answer 2
            //   Answer 3
            //☜ Answer 4

            var to = t.Int32();
            var bo = b.Int32();
            var be = -1;
            var ca = -1;

            if (begin is IConstExpression beg)
                be = beg.Int32();

            if (cancel is IConstExpression can)
                ca = can.Int32();

            var lines = SplitMonologue(message);

            for (var i = 0; i < lines.Length; i++)
            {
                sw.Append("//");
                if (i >= to && i <= bo)
                {
                    if (i == be)
                        sw.Append("☞ ");
                    else if (i == ca)
                        sw.Append("☜ ");
                    else
                        sw.Append("   ");
                }
                else
                {
                    sw.Append(" ");
                }

                sw.AppendLine(lines[i]);
            }
        }

        public static void FormatGlobalGet<T>(GlobalVariableId<T> globalVariable, int[] knownVariables, ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext) where T : unmanaged
        {
            if (knownVariables == null || Array.BinarySearch(knownVariables, globalVariable.VariableId) < 0)
            {
                sw.Append("(");
                sw.Append(GlobalVariableId<T>.TypeName);
                sw.Append(")");
                sw.Append("G");
            }
            else
            {
                sw.Append("G");
                sw.Append(GlobalVariableId<T>.TypeName);
            }

            sw.Append("[");
            sw.Append(globalVariable.VariableId.ToString(CultureInfo.InvariantCulture));
            sw.Append("]");
        }

        public static void FormatGlobalSet<T>(GlobalVariableId<T> globalVariable, IJsmExpression value, int[] knownVariables, ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext) where T : unmanaged
        {
            sw.Append("G");

            if (knownVariables == null || Array.BinarySearch(knownVariables, globalVariable.VariableId) < 0)
            {
                sw.Append("[");
                sw.Append(globalVariable.VariableId.ToString(CultureInfo.InvariantCulture));
                sw.Append("]");
                sw.Append(" = ");
                sw.Append("(");
                sw.Append(GlobalVariableId<T>.TypeName);
                sw.Append(")");
            }
            else
            {
                sw.Append(GlobalVariableId<T>.TypeName);
                sw.Append("[");
                sw.Append(globalVariable.VariableId.ToString(CultureInfo.InvariantCulture));
                sw.Append("]");
                sw.Append(" = ");
            }

            value.Format(sw, formatterContext, executionContext);
            sw.AppendLine(";");
        }

        public static void FormatMonologue(ScriptWriter sw, string message)
        {
            if (!sw.HasWhiteLine)
                sw.AppendLine();

            foreach (var str in SplitMonologue(message))
            {
                if (string.IsNullOrEmpty(str))
                    continue;

                sw.Append("// ");
                sw.AppendLine(str);
            }
        }

        public static string[] SplitMonologue(string message) => message.Split(Separators, StringSplitOptions.None);

        #endregion Methods

        #region Classes

        public sealed class Formatter
        {
            #region Constructors

            public Formatter(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                Sw = sw;
                FormatterContext = formatterContext;
                ExecutionContext = executionContext;
            }

            #endregion Constructors

            #region Properties

            public IServices ExecutionContext { get; }
            public IScriptFormatterContext FormatterContext { get; }
            public ScriptWriter Sw { get; }

            #endregion Properties

            #region Methods

            public Formatter Await()
            {
                Sw.Append("await ");

                return this;
            }

            public Formatter CommentLine(string text)
            {
                if (!Sw.HasWhiteLine)
                    Sw.AppendLine();

                Sw.Append("// ");
                Sw.AppendLine(text);
                return this;
            }

            public MethodFormatter Method(string methodName)
            {
                Sw.Append("this.");
                Sw.Append(methodName);
                Sw.Append("(");
                return new MethodFormatter(this);
            }

            public PropertyFormatter Property(string methodName)
            {
                Sw.Append("this.");
                Sw.Append(methodName);

                return new PropertyFormatter(this);
            }

            public StaticTypeFormatter StaticType(string typeName)
            {
                Sw.Append(typeName);
                return new StaticTypeFormatter(this);
            }

            #endregion Methods
        }

        public sealed class MethodFormatter
        {
            #region Fields

            private bool _hasArguments = false;

            #endregion Fields

            #region Constructors

            public MethodFormatter(Formatter formatter) => Formatter = formatter;

            #endregion Constructors

            #region Properties

            public Formatter Formatter { get; }

            #endregion Properties

            #region Methods

            public MethodFormatter Argument(string argumentName, int argumentValue) => Argument(argumentName, argumentValue.ToString(CultureInfo.InvariantCulture));

            public MethodFormatter Argument(string argumentName, bool argumentValue) => Argument(argumentName, argumentValue ? "true" : "false");

            public MethodFormatter Argument(string argumentName, IJsmExpression argumentExpression)
            {
                var sw = Formatter.Sw;

                if (_hasArguments)
                    sw.Append(", ");

                if (argumentName != null)
                {
                    sw.Append(argumentName);
                    sw.Append(": ");
                }

                argumentExpression.Format(sw, Formatter.FormatterContext, Formatter.ExecutionContext);

                _hasArguments = true;
                return this;
            }

            public MethodFormatter Argument<T>(string argumentName, IJsmExpression argumentExpression)
            {
                var sw = Formatter.Sw;

                if (_hasArguments)
                    sw.Append(", ");

                if (argumentName != null)
                {
                    sw.Append(argumentName);
                    sw.Append(": ");
                }

                sw.Append("(");
                sw.Append(typeof(T).Name);
                sw.Append(")");

                argumentExpression.Format(sw, Formatter.FormatterContext, Formatter.ExecutionContext);

                _hasArguments = true;
                return this;
            }

            public MethodFormatter Argument(string argumentValue)
            {
                var sw = Formatter.Sw;

                if (_hasArguments)
                    sw.Append(", ");

                sw.Append(argumentValue);

                _hasArguments = true;
                return this;
            }

            public MethodFormatter Argument(string argumentName, string argumentValue)
            {
                var sw = Formatter.Sw;

                if (_hasArguments)
                    sw.Append(", ");

                sw.Append(argumentName);
                sw.Append(": ");
                sw.Append(argumentValue);

                _hasArguments = true;
                return this;
            }

            public void Comment(string nativeName)
            {
                Formatter.Sw.Append("); // ");
                Formatter.Sw.AppendLine(nativeName);
            }

            public MethodFormatter Enum<T>(T argumentValue) where T : struct => Argument(typeof(T).Name + '.' + argumentValue);

            public MethodFormatter Enum<T>(IJsmExpression argumentExpression) where T : struct
            {
                if (argumentExpression is IConstExpression expr)
                    return Argument(typeof(T).Name + '.' + (T)(object)expr.Int32());

                return Argument<T>(null, argumentExpression);
            }

            #endregion Methods
        }

        public sealed class PropertyFormatter
        {
            #region Constructors

            public PropertyFormatter(Formatter formatter) => Formatter = formatter;

            #endregion Constructors

            #region Properties

            public Formatter Formatter { get; }

            #endregion Properties

            #region Methods

            public PropertyFormatter Assign(bool value)
            {
                var sw = Formatter.Sw;
                sw.Append(" = ");
                sw.Append(value ? "true" : "false");
                sw.Append(";");
                return this;
            }

            public PropertyFormatter Assign(long value)
            {
                var sw = Formatter.Sw;
                sw.Append(" = ");
                sw.Append(value.ToString(CultureInfo.InvariantCulture));
                sw.Append(";");
                return this;
            }

            public PropertyFormatter Assign(IJsmExpression assignExpression)
            {
                var sw = Formatter.Sw;
                sw.Append(" = ");
                assignExpression.Format(sw, Formatter.FormatterContext, Formatter.ExecutionContext);
                sw.Append(";");
                return this;
            }

            public void Comment(string nativeName)
            {
                Formatter.Sw.Append(" // ");
                Formatter.Sw.AppendLine(nativeName);
            }

            public MethodFormatter Method(string methodName)
            {
                var sw = Formatter.Sw;
                sw.Append(".");
                sw.Append(methodName);
                sw.Append("(");
                return new MethodFormatter(Formatter);
            }

            public PropertyFormatter Property(string propertyName)
            {
                Formatter.Sw.Append(".");
                Formatter.Sw.Append(propertyName);

                return new PropertyFormatter(Formatter);
            }

            #endregion Methods
        }

        public sealed class StaticTypeFormatter
        {
            #region Constructors

            public StaticTypeFormatter(Formatter formatter) => Formatter = formatter;

            #endregion Constructors

            #region Properties

            public Formatter Formatter { get; }

            #endregion Properties

            #region Methods

            public MethodFormatter Method(string methodName)
            {
                var sw = Formatter.Sw;
                sw.Append(".");
                sw.Append(methodName);
                sw.Append("(");
                return new MethodFormatter(Formatter);
            }

            public PropertyFormatter Property(string propertyName)
            {
                Formatter.Sw.Append(".");
                Formatter.Sw.Append(propertyName);

                return new PropertyFormatter(Formatter);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}