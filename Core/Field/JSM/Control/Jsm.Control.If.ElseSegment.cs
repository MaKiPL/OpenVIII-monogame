using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Control
        {
            public sealed partial class If
            {
                private sealed class ElseSegment : Segment
                {
                    public ElseSegment(Int32 from, Int32 to)
                        : base(from, to)
                    {
                    }

                    public override void ToString(StringBuilder sb)
                    {
                        sb.AppendLine("else");
                        FormatBranch(sb, GetBodyInstructions());
                    }

                    public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.AppendLine("else");
                        FormatBranch(sw, formatterContext, services, GetBodyInstructions());
                    }

                    public IEnumerable<IJsmInstruction> GetBodyInstructions()
                    {
                        return _list;
                    }
                }
            }
        }
    }
}