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

                public sealed class ElseIfSegment : Segment
                {
                    #region Constructors

                    public ElseIfSegment(int from, int to)
                        : base(from, to)
                    {
                    }

                    #endregion Constructors

                    #region Properties

                    public JPF Jpf => ((JPF)_list[0]);

                    #endregion Properties

                    #region Methods

                    public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("else if(");
                        ((JPF)_list[0]).Format(sw, formatterContext, services);
                        sw.AppendLine(")");
                        FormatBranch(sw, formatterContext, services, GetBodyInstructions());
                    }

                    public IEnumerable<IJsmInstruction> GetBodyInstructions() => _list.Skip(1);

                    public override void ToString(StringBuilder sb)
                    {
                        sb.Append("else if(");
                        sb.Append(Jpf);
                        sb.AppendLine(")");
                        FormatBranch(sb, GetBodyInstructions());
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