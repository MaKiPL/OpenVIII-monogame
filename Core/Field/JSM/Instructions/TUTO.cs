namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Show Tutorial
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/183_UNKNOWN18"/>
    public sealed class TUTO : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Tutorial ID
        /// </summary>
        private readonly IJsmExpression _tutorialID;

        #endregion Fields

        #region Constructors

        public TUTO(IJsmExpression tutorialID) => _tutorialID = tutorialID;

        public TUTO(int parameter, IStack<IJsmExpression> stack)
            : this(
                tutorialID: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(TUTO)}({nameof(_tutorialID)}: {_tutorialID})";

        #endregion Methods
    }
}