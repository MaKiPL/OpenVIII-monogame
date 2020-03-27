namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Turn Particle on.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/14E_PARTICLEON&action=edit&redlink=1"/>
    public sealed class PARTICLEON : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public PARTICLEON(IJsmExpression arg0) => _arg0 = arg0;

        public PARTICLEON(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(PARTICLEON)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}