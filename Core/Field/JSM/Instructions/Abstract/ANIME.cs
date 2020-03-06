using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class ANIME : JsmInstruction
    {
        /// <summary>
        /// Model Animation BattleID
        /// </summary>
        protected readonly Int32 _animationId;

        public ANIME(Int32 animationId)
        {
            _animationId = animationId;

        }
        public ANIME(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId)
        { }
        public abstract override string ToString();
    }
}
