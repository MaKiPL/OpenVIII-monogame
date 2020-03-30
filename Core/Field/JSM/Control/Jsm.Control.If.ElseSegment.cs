using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Generic;
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

                public sealed class ElseSegment : Segment
                {
                    #region Constructors

                    public ElseSegment(int from, int to)
                        : base(from, to)
                    {
                    }

                    #endregion Constructors

                    #region Methods

                    public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.AppendLine("else");
                        FormatBranch(sw, formatterContext, services, GetBodyInstructions());
                    }

                    public IEnumerable<IJsmInstruction> GetBodyInstructions() => _list;

                    public override void ToString(StringBuilder sb)
                    {
                        sb.AppendLine("else");
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