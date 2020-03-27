namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// stop shade?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0D1_BGSHADESTOP&action=edit&redlink=1"/>
    public sealed class BGSHADESTOP : JsmInstruction
    {
        #region Constructors

        public BGSHADESTOP()
        {
        }

        public BGSHADESTOP(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BGSHADESTOP)}()";

        #endregion Methods
    }
}