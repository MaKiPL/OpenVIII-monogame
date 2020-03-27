namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Push signed from memory (byte)</para>
            /// <para>Push the first signed byte from Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/010_PSHSM_B"/>
            public sealed class PSHSM_B : IJsmExpression
            {
                #region Fields

                /// <summary>
                /// Memory address.
                /// </summary>
                private GlobalVariableId<sbyte> _globalVariable;

                #endregion Fields

                #region Constructors

                public PSHSM_B(GlobalVariableId<sbyte> globalVariable) => _globalVariable = globalVariable;

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

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalGet(_globalVariable, null, sw, formatterContext, services);

                public override string ToString() => $"{nameof(PSHSM_B)}({nameof(_globalVariable)}: {_globalVariable})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}