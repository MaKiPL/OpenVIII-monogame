namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Push from memory (byte)</para>
            /// <para>Push first byte from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00A_PSHM_B"/>
            public sealed class PSHM_B : IJsmExpression
            {
                #region Fields

                private GlobalVariableId<byte> _globalVariable;

                #endregion Fields

                #region Constructors

                /// <summary>
                /// Push first byte from Argument onto the stack.
                /// </summary>
                /// <param name="globalVariable"></param>
                public PSHM_B(GlobalVariableId<byte> globalVariable) => _globalVariable = globalVariable;

                #endregion Constructors

                #region Methods

                public long Calculate(IServices services) => ServiceId.Global[services].Get(_globalVariable);

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

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalGet(_globalVariable, Jsm.GlobalUInt8, sw, formatterContext, services);

                public override string ToString() => $"{nameof(PSHM_B)}({nameof(_globalVariable)}: {_globalVariable})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}