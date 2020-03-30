namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>This is some kind of weird wait command. UNKNOWN12 is probably the waitsync for it. The second parameter is definitely a frame Count, but I have no ides what the first parameter is (it's usually 0).</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/176_UNKNOWN11"/>
    internal sealed class Unknown11 : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Unknown usually 0.
        /// </summary>
        private readonly IJsmExpression _arg0;

        /// <summary>
        /// Frame Count
        /// </summary>
        private readonly IJsmExpression _frames;

        #endregion Fields

        #region Constructors

        public Unknown11(IJsmExpression arg0, IJsmExpression frames)
        {
            _arg0 = arg0;
            _frames = frames;
        }

        public Unknown11(int parameter, IStack<IJsmExpression> stack)
            : this(
                frames: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown11)}({nameof(_arg0)}: {_arg0}, {nameof(_frames)}: {_frames})";

        #endregion Methods
    }
}