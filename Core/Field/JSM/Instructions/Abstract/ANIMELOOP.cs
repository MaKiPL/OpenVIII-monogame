namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class ANIMELOOP : ANIME
    {
        #region Fields

        /// <summary>
        /// First frame of animation
        /// </summary>
        protected readonly int _firstFrame;

        /// <summary>
        /// Last frame of animation
        /// </summary>
        protected readonly int _lastFrame;

        #endregion Fields

        #region Constructors

        public ANIMELOOP(int animationId, int firstFrame, int lastFrame) : base(animationId)
        {
            _firstFrame = firstFrame;
            _lastFrame = lastFrame;
        }

        public ANIMELOOP(int animationId, IStack<IJsmExpression> stack)
            : this(animationId,
                lastFrame: ((IConstExpression)stack.Pop()).Int32(),
                firstFrame: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors
    }
}