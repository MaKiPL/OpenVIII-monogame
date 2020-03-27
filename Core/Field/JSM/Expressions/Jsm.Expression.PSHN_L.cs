using System.Globalization;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Push Numeric (long)</para>
            /// <para>Push Argument onto the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/007_PSHN_L"/>
            public sealed class PSHN_L : IConstExpression
            {
                #region Constructors

                public PSHN_L(int value) => Value = value;

                #endregion Constructors

                #region Properties

                public int Value { get; }
                long IConstExpression.Value => Value;

                #endregion Properties

                #region Methods

                public long Calculate(IServices services) => Value;

                public IJsmExpression Evaluate(IServices services) => this;

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Append(Value.ToString(CultureInfo.InvariantCulture));

                public ILogicalExpression LogicalInverse() => ValueExpression.Create(Value == 0 ? 1 : 0);

                public override string ToString() => $"{nameof(PSHN_L)}({nameof(Value)}: {Value})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}