using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Expression
        {
            /// <summary>
            /// <para>Push from memory (word)</para>
            /// <para>Push this first two bytes (word) from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00C_PSHM_W"/>
            public sealed class PSHM_W : IJsmExpression
            {
                /// <summary>
                /// Memory address
                /// </summary>
                private GlobalVariableId<UInt16> _globalVariable;

                public PSHM_W(GlobalVariableId<UInt16> globalVariable)
                {
                    _globalVariable = globalVariable;
                }

                public override String ToString()
                {
                    return $"{nameof(PSHM_W)}({nameof(_globalVariable)}: {_globalVariable})";
                }

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                {
                    FormatHelper.FormatGlobalGet(_globalVariable, Jsm.GlobalUInt16, sw, formatterContext, services);
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