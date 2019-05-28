using System;
using FF8.Core;
using FF8.JSM.Format;

namespace FF8.JSM
{
    public static partial class Jsm
    {
        public sealed class GameScript
        {
            public Int32 ScriptId { get; }
            public Jsm.ExecutableSegment Segment { get; }

            public GameScript(Int32 scriptId, Jsm.ExecutableSegment segment)
            {
                ScriptId = scriptId;
                Segment = segment;
            }

            public void FormatMethod(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                String methodName = GetMethodName(formatterContext);

                sw.AppendLine($"public void {methodName}()");
                {
                    sw.AppendLine("{");
                    sw.Indent++;

                    FormatMethodBody(sw, formatterContext, executionContext);

                    sw.Indent--;
                    sw.AppendLine("}");
                }
            }

            public void FormatMethodBody(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                Segment.Format(sw, formatterContext, executionContext);
            }

            private String GetMethodName(IScriptFormatterContext formatterContext)
            {
                formatterContext.GetObjectScriptNamesById(ScriptId, out _, out String methodName);
                if (Char.IsLower(methodName[0]))
                    methodName = Char.ToUpperInvariant(methodName[0]) + methodName.Substring(1);
                return methodName;
            }
        }
    }
}