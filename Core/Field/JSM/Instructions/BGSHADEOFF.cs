namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// turn off shade?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/117_BGSHADEOFF&action=edit&redlink=1"/>
    public sealed class BGSHADEOFF : JsmInstruction
    {
        #region Constructors

        public BGSHADEOFF()
        {
        }

        public BGSHADEOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BGSHADEOFF)}()";

        #endregion Methods
    }
}