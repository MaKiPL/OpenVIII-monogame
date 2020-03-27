namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// BGanime with R unsure the structure copied from BGanime assuming they are related.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/096_RBGANIME&action=edit&redlink=1"/>
    public sealed class RBGANIME : Abstract.BGANIME
    {
        #region Constructors

        public RBGANIME(IJsmExpression firstFrame, IJsmExpression lastFrame) : base(firstFrame, lastFrame)
        {
        }

        public RBGANIME(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(RBGANIME)}({nameof(_firstFrame)}: {_firstFrame}, {nameof(_lastFrame)}: {_lastFrame})";

        #endregion Methods
    }
}