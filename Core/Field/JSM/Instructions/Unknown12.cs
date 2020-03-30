namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// End Unknown11
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/177_UNKNOWN12"/>
    public sealed class Unknown12 : JsmInstruction
    {
        #region Constructors

        public Unknown12()
        {
        }

        public Unknown12(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown12)}()";

        #endregion Methods
    }
}