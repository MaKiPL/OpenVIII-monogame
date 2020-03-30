namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Resume script, Play looping animation
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/035_RANIMELOOP"/>
    public sealed class RANIMELOOP : Abstract.ANIME
    {
        #region Constructors

        public RANIMELOOP(int animationId) : base(animationId)
        {
        }

        public RANIMELOOP(int animationId, IStack<IJsmExpression> stack) : base(animationId, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(RANIMELOOP)}({nameof(_animationId)}: {_animationId})";

        #endregion Methods
    }
}