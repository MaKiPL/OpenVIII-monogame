using OpenVIII.Fields.Scripts.Instructions;
using System;
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

            public sealed partial class If : IJsmControl
            {
                #region Fields

                private readonly List<JsmInstruction> _instructions;
                private readonly List<ElseIfSegment> ElseIfRanges = new List<ElseIfSegment>();
                private readonly IfSegment IfRange;
                private ElseSegment ElseRange;

                #endregion Fields

                #region Constructors

                public If(List<JsmInstruction> instructions, int from, int to)
                {
                    _instructions = instructions;
                    IfRange = new IfSegment(from, to, this)
                    {
                        _instructions[from]
                    };
                }

                #endregion Constructors

                #region Methods

                public void AddElse(int from, int to)
                {
                    if (ElseRange != null)
                        throw new InvalidOperationException($"Cannot replace existing else ({ElseRange}) by a new one.");

                    ElseRange = new ElseSegment(from, to);
                }

                public void AddIf(int from, int to)
                {
                    var elseIf = new ElseIfSegment(from, to);
                    elseIf.Add(_instructions[from]);
                    ElseIfRanges.Add(elseIf);
                }

                public IEnumerable<Segment> EnumerateSegments()
                {
                    yield return IfRange;

                    foreach (var block in EnumerateElseBlocks())
                        yield return block;
                }

                public override string ToString()
                {
                    var sb = new StringBuilder();
                    FormatIf(sb, IfRange);
                    FormatBranch(sb, IfRange);
                    foreach (var item in ElseIfRanges)
                    {
                        FormatElseIf(sb, item);
                        FormatBranch(sb, item);
                    }

                    if (ElseRange != null)
                    {
                        FormatElse(sb, ElseRange);
                        FormatBranch(sb, ElseRange);
                    }

                    return sb.ToString();
                }

                private IEnumerable<Segment> EnumerateElseBlocks()
                {
                    foreach (var item in ElseIfRanges)
                        yield return item;

                    if (ElseRange != null)
                        yield return ElseRange;
                }

                private void FormatBranch(StringBuilder sb, Segment range)
                {
                    sb.AppendLine("{");
                    for (var i = range.From + 1; i < range.To; i++)
                    {
                        var instruction = _instructions[i];
                        sb.Append('\t').AppendLine(instruction.ToString());
                    }

                    sb.AppendLine("}");
                }

                private void FormatElse(StringBuilder sb, Segment elseRange) => sb.AppendLine("else");

                private void FormatElseIf(StringBuilder sb, Segment elseIfRange)
                {
                    sb.Append("else if(");
                    sb.Append((JPF)_instructions[elseIfRange.From]);
                    sb.AppendLine(")");
                }

                private void FormatIf(StringBuilder sb, Segment ifRange)
                {
                    sb.Append("if(");
                    sb.Append((JPF)_instructions[ifRange.From]);
                    sb.AppendLine(")");
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}