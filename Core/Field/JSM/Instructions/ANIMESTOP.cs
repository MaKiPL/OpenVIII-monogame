namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Resume script, controlled animation. Returns this entity to its base animation.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/045_ANIMESTOP"/>
    public sealed class ANIMESTOP : JsmInstruction
    {
        #region Constructors

        public ANIMESTOP()
        {
        }

        public ANIMESTOP(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ANIMESTOP)}()";

        #endregion Methods
    }
}