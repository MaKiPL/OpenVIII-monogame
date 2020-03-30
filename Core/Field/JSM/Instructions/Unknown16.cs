namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Draw Point ID / Assigns this draw point an ID. Draw points with identical IDs share Full/Drained status.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/181_UNKNOWN16"/>
    public sealed class Unknown16 : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Draw point ID
        /// </summary>
        private readonly IJsmExpression _drawPointID;

        #endregion Fields

        #region Constructors

        public Unknown16(IJsmExpression drawPointID) => _drawPointID = drawPointID;

        public Unknown16(int parameter, IStack<IJsmExpression> stack)
            : this(
                drawPointID: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown16)}({nameof(_drawPointID)}: {_drawPointID})";

        #endregion Methods
    }
}