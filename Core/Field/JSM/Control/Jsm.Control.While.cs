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

            public sealed partial class While : IJsmControl
            {
                #region Fields

                private readonly List<JsmInstruction> _instructions;
                private readonly WhileSegment _segment;

                #endregion Fields

                #region Constructors

                public While(List<JsmInstruction> instructions, int from, int to)
                {
                    _instructions = instructions;
                    _segment = new WhileSegment(from, to)
                    {
                        _instructions[from]
                    };
                }

                #endregion Constructors

                #region Methods

                public IEnumerable<Segment> EnumerateSegments()
                {
                    yield return _segment;
                }

                public override string ToString()
                {
                    var sb = new StringBuilder();

                    sb.Append("while(");
                    sb.Append((JPF)_instructions[_segment.From]);
                    sb.AppendLine(")");

                    sb.AppendLine("{");
                    for (var i = _segment.From + 1; i < _segment.To; i++)
                    {
                        var instruction = _instructions[i];
                        sb.Append('\t').AppendLine(instruction.ToString());
                    }

                    sb.AppendLine("}");

                    return base.ToString();
                }

                #endregion Methods

                #region Classes

                public sealed class WhileSegment : ExecutableSegment
                {
                    #region Constructors

                    public WhileSegment(int from, int to)
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
                        sw.Append("while(");
                        ((JPF)_list[0]).Format(sw, formatterContext, services);
                        sw.AppendLine(")");
                        FormatBranch(sw, formatterContext, services, _list.Skip(1));
                    }

                    public IEnumerable<IJsmInstruction> GetBodyInstructions() => _list.Skip(1);

                    public override IScriptExecutor GetExecuter() => new Executor(this);

                    public override void ToString(StringBuilder sb)
                    {
                        sb.Append("while(");
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