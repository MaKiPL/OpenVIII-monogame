using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Expression
        {
            /// <summary>
            /// <para>Push signed from memory (byte)</para>
            /// <para>Push the first signed byte from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/010_PSHSM_B"/>
            public sealed class PSHSM_B : IJsmExpression
            {
                /// <summary>
                /// Memory address.
                /// </summary>
                private GlobalVariableId<SByte> _globalVariable;

                public PSHSM_B(GlobalVariableId<SByte> globalVariable)
                {
                    _globalVariable = globalVariable;
                }

                public override String ToString()
                {
                    return $"{nameof(PSHSM_B)}({nameof(_globalVariable)}: {_globalVariable})";
                }

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                {
                    FormatHelper.FormatGlobalGet(_globalVariable, null, sw, formatterContext, services);
                }

                public IJsmExpression Evaluate(IServices services)
                {
                    var global = ServiceId.Global[services];
                    if (global.IsSupported)
                    {
                        var value = global.Get(_globalVariable);
                        return ValueExpression.Create(value);
                    }
                    return this;
                }

                public Int64 Calculate(IServices services)
                {
                    return ServiceId.Global[services].Get(_globalVariable);
                }
            }
        }
    }
}