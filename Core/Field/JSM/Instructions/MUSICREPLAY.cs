namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Replay music?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/141_MUSICREPLAY&action=edit&redlink=1"/>
    public sealed class MUSICREPLAY : JsmInstruction
    {
        #region Constructors

        public MUSICREPLAY()
        {
        }

        public MUSICREPLAY(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MUSICREPLAY)}()";

        #endregion Methods
    }
}