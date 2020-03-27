namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// I think this pushes the current sample time of the playing music track (or something like that) into temporary variable 0. UNKNOWN10 is always called just before a map transition, and after the transition, the variables storing the result (bytes 458 and 459) are the parameter of MUSICSKIP.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/175_UNKNOWN10"/>
    public sealed class Unknown10 : JsmInstruction
    {
        #region Constructors

        public Unknown10()
        {
        }

        public Unknown10(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown10)}()";

        #endregion Methods
    }
}