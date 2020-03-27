namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public sealed class GameScript
        {
            #region Constructors

            public GameScript(int scriptId, Jsm.ExecutableSegment segment)
            {
                ScriptId = scriptId;
                Segment = segment;
            }

            #endregion Constructors

            #region Properties

            public int ScriptId { get; }
            public Jsm.ExecutableSegment Segment { get; }

            #endregion Properties

            #region Methods

            public void FormatMethod(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                var methodName = GetMethodName(formatterContext);

                sw.AppendLine($"public void {methodName}()");
                {
                    sw.AppendLine("{");
                    sw.Indent++;

                    FormatMethodBody(sw, formatterContext, executionContext);

                    sw.Indent--;
                    sw.AppendLine("}");
                }
            }

            public void FormatMethodBody(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext) => Segment.Format(sw, formatterContext, executionContext);

            public override string ToString() => $"{{{ScriptId}, {Segment}}}";

            private string GetMethodName(IScriptFormatterContext formatterContext)
            {
                formatterContext.GetObjectScriptNamesById(ScriptId, out _, out var methodName);
                if (char.IsLower(methodName[0]))
                    methodName = char.ToUpperInvariant(methodName[0]) + methodName.Substring(1);
                return methodName;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}