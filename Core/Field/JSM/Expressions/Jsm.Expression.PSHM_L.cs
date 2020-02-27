using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Expression
        {
            /// <summary>
            /// <para>Push from memory (long)</para>
            /// <para>Push the first four bytes (long) from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00E_PSHM_L"/>
            public sealed class PSHM_L : IJsmExpression
            {
                /// <summary>
                /// Memory address.
                /// </summary>
                private GlobalVariableId<UInt32> _globalVariable;

                public PSHM_L(GlobalVariableId<UInt32> globalVariable)
                {
                    _globalVariable = globalVariable;
                }

                public override String ToString()
                {
                    return $"{nameof(PSHM_L)}({nameof(_globalVariable)}: {_globalVariable})";
                }

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                {
                    FormatHelper.FormatGlobalGet(_globalVariable, Jsm.GlobalUInt32, sw, formatterContext, services);
                }

                public IJsmExpression Evaluate(IServices services)
                {
                    IGlobalVariableService global = ServiceId.Global[services];
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