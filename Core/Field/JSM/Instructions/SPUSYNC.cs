namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>SPU Sync</para>
    /// <para>Pauses this script until frame Count frames have passed since SPUREADY was called.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/164_SPUSYNC"/>
    public sealed class SPUSYNC : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Frame Count
        /// </summary>
        private readonly IJsmExpression _frameCount;

        #endregion Fields

        #region Constructors

        public SPUSYNC(IJsmExpression frameCount) => _frameCount = frameCount;

        public SPUSYNC(int parameter, IStack<IJsmExpression> stack)
            : this(
                frameCount: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SPUSYNC)}({nameof(_frameCount)}: {_frameCount})";

        #endregion Methods
    }
}