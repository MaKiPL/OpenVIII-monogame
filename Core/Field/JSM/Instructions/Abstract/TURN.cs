namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class TURN : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Angle
        /// </summary>
        protected IJsmExpression _angle;

        /// <summary>
        /// Duration of turn (frames)
        /// </summary>
        protected IJsmExpression _frames;

        #endregion Fields

        #region Constructors

        public TURN(IJsmExpression frames, IJsmExpression angle)
        {
            _frames = frames;
            _angle = angle;
        }

        public TURN(int parameter, IStack<IJsmExpression> stack)
            : this(
                angle: stack.Pop(),
                frames: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public abstract override string ToString();

        #endregion Methods
    }
}