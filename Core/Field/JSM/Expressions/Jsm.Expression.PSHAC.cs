namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Push actor code???</para>
            /// <para>Push the entity ID of an actor onto the stack. This is always used before a call to CTURN to set which character will be faced. IDK what happens when you try to use literals for CTURN or why this function is necessary.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/013_PSHAC"/>
            public sealed class PSHAC : IJsmExpression, IConstExpression
            {
                #region Fields

                /// <summary>
                /// ID of the entity to reference
                /// </summary>
                private Jsm.FieldObjectId _fieldObjectId;

                #endregion Fields

                #region Constructors

                /// <summary>
                /// Push the entity ID of an actor onto the stack. This is always used before a call to CTURN to set which character will be faced. IDK what happens when you try to use literals for CTURN or why this function is necessary.
                /// </summary>
                /// <param name="fieldObjectId">Char value</param>
                public PSHAC(Jsm.FieldObjectId fieldObjectId) => _fieldObjectId = fieldObjectId;

                #endregion Constructors

                #region Properties

                long IConstExpression.Value => _fieldObjectId.Value;

                #endregion Properties

                #region Methods

                public long Calculate(IServices services) => _fieldObjectId.Value;

                public IJsmExpression Evaluate(IServices services) => this;

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                {
                    var name = formatterContext.GetObjectNameByIndex(_fieldObjectId.Value);
                    sw.Append($"typeof({name})");
                }

                ILogicalExpression ILogicalExpression.LogicalInverse() => ValueExpression.Create(_fieldObjectId.Value == 0 ? 1 : 0);

                public override string ToString() => $"{nameof(PSHAC)}({nameof(_fieldObjectId)}: {_fieldObjectId})";

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}