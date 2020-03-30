namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// BGanime with R and LOOP unsure the structure copied from BGanime assuming they are related.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/097_RBGANIMELOOP&action=edit&redlink=1"/>
    public sealed class RBGANIMELOOP : Abstract.BGANIME
    {
        #region Constructors

        public RBGANIMELOOP(IJsmExpression firstFrame, IJsmExpression lastFrame) : base(firstFrame, lastFrame)
        {
        }

        public RBGANIMELOOP(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(RBGANIMELOOP)}({nameof(_firstFrame)}: {_firstFrame}, {nameof(_lastFrame)}: {_lastFrame})";

        #endregion Methods
    }
}