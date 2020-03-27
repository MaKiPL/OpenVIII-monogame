namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class PREQ : REQ
    {
        #region Constructors

        public PREQ(int objectIndex, IStack<IJsmExpression> stack) : base(objectIndex, stack)
        {
        }

        public PREQ(int objectIndex, int priority, int scriptId) : base(objectIndex, priority, scriptId)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The ID of the current party member Entity (0, 1 or 2).
        /// </summary>
        protected int _partyId => checked((byte)_objectIndex);

        #endregion Properties
    }
}