using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public sealed class GameObject
        {
            #region Constructors

            public GameObject(int id, IReadOnlyList<GameScript> scripts)
            {
                Id = id;
                Scripts = scripts;
            }

            #endregion Constructors

            #region Properties

            public int Id { get; }
            public IReadOnlyList<GameScript> Scripts { get; }

            #endregion Properties

            #region Methods

            public void FormatType(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                formatterContext.GetObjectScriptNamesById(Id, out var typeName, out _);
                sw.AppendLine($"public sealed class {typeName}");
                {
                    sw.AppendLine("{");
                    sw.Indent++;

                    if (Scripts.Count > 0)
                    {
                        FormatConstructor(typeName, sw, formatterContext, executionContext);

                        foreach (var script in Scripts.Skip(1))
                        {
                            sw.AppendLine();
                            script.FormatMethod(sw, formatterContext, executionContext);
                        }
                    }

                    sw.Indent--;
                    sw.AppendLine("}");
                }
            }

            public override string ToString() => $"{{{Id}, {nameof(Scripts)} {nameof(Scripts.Count)} = {Scripts.Count}}}";

            private void FormatConstructor(string typeName, ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
            {
                sw.AppendLine($"private readonly {nameof(IServices)} _ctx;");
                sw.AppendLine();

                sw.AppendLine($"public {typeName}({nameof(IServices)} executionContext)");
                {
                    sw.AppendLine("{");
                    sw.Indent++;

                    sw.AppendLine("_ctx = executionContext;");
                    Scripts[0].FormatMethodBody(sw, formatterContext, executionContext);

                    sw.Indent--;
                    sw.AppendLine("}");
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}