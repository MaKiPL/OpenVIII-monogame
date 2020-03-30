namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Push from memory (long)</para>
            /// <para>Push the first four bytes (long) from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00E_PSHM_L"/>
            public sealed class PSHM_L : IJsmExpression
            {
                #region Fields

                /// <summary>
                /// Memory address.
                /// </summary>
                private GlobalVariableId<uint> _globalVariable;

                #endregion Fields

                #region Constructors

                public PSHM_L(GlobalVariableId<uint> globalVariable) => _globalVariable = globalVariable;

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

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalGet(_globalVariable, Jsm.GlobalUInt32, sw, formatterContext, services);

                public override string ToString() => $"{nameof(PSHM_L)}({nameof(_globalVariable)}: {_globalVariable})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}