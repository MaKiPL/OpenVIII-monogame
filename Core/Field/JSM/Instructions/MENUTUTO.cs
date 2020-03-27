namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// MENU tutorial? used in bgroom_5 only which is your computer in the classroom.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15A_MENUTUTO&action=edit&redlink=1"/>
    public sealed class MENUTUTO : JsmInstruction
    {
        #region Constructors

        public MENUTUTO()
        {
        }

        public MENUTUTO(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MENUTUTO)}()";

        #endregion Methods
    }
}