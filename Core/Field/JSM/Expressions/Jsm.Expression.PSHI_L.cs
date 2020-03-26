using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Expression
        {
            /// <summary>
            /// <para>Push from Temp List (long)</para>
            /// <para>Push the value (long) at index position Argument in the Temp List onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/008_PSHI_L"/>
            public sealed class PSHI_L : IJsmExpression
            {
                /// <summary>
                /// Index position in the Temp List (0 &lt;= Argument &lt; 8).
                /// </summary>
                private ScriptResultId _index;

                /// <summary>
                /// Push the value (Int32) at index position in the Temp List onto the stack.
                /// </summary>
                /// <param name="index">Index position in the Temp List (0...7).</param>
                public PSHI_L(ScriptResultId index) => _index = index;

                public override String ToString() => $"{nameof(PSHI_L)}({nameof(_index)}: {_index})";

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Append($"R{_index.ResultId}");

                public IJsmExpression Evaluate(IServices services)
                {
                    var interaction = ServiceId.Interaction[services];
                    if (interaction.IsSupported)
                    {
                        var value = interaction[_index];
                        return ValueExpression.Create(value);
                    }

                    return this;
                }

                public Int64 Calculate(IServices services) => ServiceId.Interaction[services][_index];
            }
        }
    }
}