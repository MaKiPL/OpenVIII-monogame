namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class BGANIME : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// First frame of animation
        /// </summary>
        protected readonly IJsmExpression _firstFrame;

        /// <summary>
        /// Last frame of animation
        /// </summary>
        protected readonly IJsmExpression _lastFrame;

        #endregion Fields

        #region Constructors

        public BGANIME(IJsmExpression firstFrame, IJsmExpression lastFrame)
        {
            _firstFrame = firstFrame;
            _lastFrame = lastFrame;
        }

        public BGANIME(int parameter, IStack<IJsmExpression> stack)
            : this(
                lastFrame: stack.Pop(),
                firstFrame: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public abstract override string ToString();

        #endregion Methods
    }
}