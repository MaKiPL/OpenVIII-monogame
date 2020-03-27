namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// door line off? disable door?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/142_DOORLINEOFF&action=edit&redlink=1"/>
    public sealed class DOORLINEOFF : JsmInstruction
    {
        #region Constructors

        public DOORLINEOFF()
        {
        }

        public DOORLINEOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(DOORLINEOFF)}()";

        #endregion Methods
    }
}