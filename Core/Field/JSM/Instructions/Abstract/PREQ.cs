using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class PREQ : REQ
    {
        public PREQ(int objectIndex, IStack<IJsmExpression> stack) : base(objectIndex, stack)
        {
        }

        public PREQ(int objectIndex, int priority, int scriptId) : base(objectIndex, priority, scriptId)
        {
        }

        /// <summary>
        /// The ID of the current party member Entity (0, 1 or 2).
        /// </summary>
        protected Int32 _partyId => checked((byte)_objectIndex);
    }
}
