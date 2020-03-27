namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class ANIME : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Model Animation ID
        /// </summary>
        protected readonly int _animationId;

        #endregion Fields

        #region Constructors

        public ANIME(int animationId) => _animationId = animationId;

        public ANIME(int animationId, IStack<IJsmExpression> stack)
            : this(animationId)
        { }

        #endregion Constructors

        #region Methods

        public abstract override string ToString();

        #endregion Methods
    }
}