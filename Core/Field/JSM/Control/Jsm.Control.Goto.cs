using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Control
        {
            #region Classes

            public sealed class Goto : IJsmControl, IFormattableScript
            {
                #region Fields

                private readonly List<JsmInstruction> _instructions;
                private readonly int _label;
                private readonly Segment _segment;

                #endregion Fields

                #region Constructors

                public Goto(List<JsmInstruction> instructions, int from, int label)
                {
                    _instructions = instructions;
                    _segment = new ExecutableSegment(from, from + 1)
                    {
                        _instructions[from]
                    };
                    _label = label;
                }

                #endregion Constructors

                #region Methods

                public IEnumerable<Segment> EnumerateSegments()
                {
                    yield return _segment;
                }

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.AppendLine($"goto LABEL{_label};");

                public override string ToString() => $"goto {_segment.From} -> {_label} ({_instructions[_label]})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}