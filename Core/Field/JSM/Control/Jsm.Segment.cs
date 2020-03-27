using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public abstract partial class Segment : IFormattableScript
        {
            #region Fields

            protected readonly List<IJsmInstruction> _list = new List<IJsmInstruction>();

            #endregion Fields

            #region Constructors

            public Segment(int from, int to)
            {
                From = from;
                To = to;
            }

            #endregion Constructors

            #region Properties

            public int From { get; }
            public int To { get; }

            #endregion Properties

            #region Methods

            public void Add(IJsmInstruction value) => _list.Add(value);

            public virtual void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatItems(sw, formatterContext, services, _list);

            public override string ToString()
            {
                var sb = new StringBuilder();
                ToString(sb);
                return sb.ToString();
            }

            public virtual void ToString(StringBuilder sb)
            {
                foreach (var item in _list)
                    sb.AppendLine(item.ToString());
            }

            protected void FormatBranch(StringBuilder sb, IEnumerable<object> items)
            {
                sb.AppendLine("{");

                var pos = sb.Length;
                AppendItems(sb, items);
                if (sb.Length == pos)
                    sb.AppendLine("// do nothing");

                sb.AppendLine("}");
            }

            protected void FormatBranch(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext, IEnumerable<IJsmInstruction> items)
            {
                sw.AppendLine("{");
                sw.Indent++;

                var state = sw.RememberState();
                FormatItems(sw, formatterContext, executionContext, items);
                if (!state.IsChanged)
                {
                    sw.AppendLine("// do nothing");
                }

                sw.Indent--;
                sw.AppendLine("}");
            }

            private static void AppendItems(StringBuilder sb, IEnumerable<object> items)
            {
                var position = -1;
                JMP lastItem = null;
                foreach (var item in items)
                {
                    if (lastItem != null)
                        throw new InvalidProgramException($"Unexpected jump: {lastItem}");

                    lastItem = item as JMP;
                    position = sb.Length;

                    sb.Append('\t').AppendLine(item.ToString());
                }

                if (lastItem != null)
                    sb.Length = position;
            }

            private static void FormatItems(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext, IEnumerable<IJsmInstruction> items)
            {
                foreach (var item in items)
                {
                    if (item is IFormattableScript formattable)
                        formattable.Format(sw, formatterContext, executionContext);
                    else
                        sw.AppendLine(item.ToString());
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}