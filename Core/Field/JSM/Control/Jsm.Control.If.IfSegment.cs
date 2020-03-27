using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Control
        {
            #region Classes

            public sealed partial class If
            {
                #region Classes

                public sealed class IfSegment : ExecutableSegment
                {
                    #region Fields

                    private readonly If _aggregator;

                    #endregion Fields

                    #region Constructors

                    public IfSegment(int from, int to, If aggregator)
                        : base(from, to) => _aggregator = aggregator;

                    #endregion Constructors

                    #region Properties

                    public JPF Jpf => ((JPF)_list[0]);

                    #endregion Properties

                    #region Methods

                    public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("if(");
                        Jpf.Format(sw, formatterContext, services);
                        sw.AppendLine(")");
                        FormatBranch(sw, formatterContext, services, GetBodyInstructions());

                        foreach (var item in _aggregator.EnumerateElseBlocks())
                            item.Format(sw, formatterContext, services);
                    }

                    public IEnumerable<IJsmInstruction> GetBodyInstructions() => _list.Skip(1);

                    public override IScriptExecutor GetExecuter() => new Executor(_aggregator);

                    public override void ToString(StringBuilder sb)
                    {
                        sb.Append("if(");
                        sb.Append((JPF)_list[0]);
                        sb.AppendLine(")");
                        FormatBranch(sb, _list.Skip(1));
                    }

                    #endregion Methods
                }

                #endregion Classes
            }

            #endregion Classes
        }

        #endregion Classes
    }
}