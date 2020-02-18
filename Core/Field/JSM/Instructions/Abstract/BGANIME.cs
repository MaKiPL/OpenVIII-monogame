using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class BGANIME : JsmInstruction
    {
        /// <summary>
        /// First frame of animation
        /// </summary>
        protected readonly IJsmExpression _firstFrame;
        /// <summary>
        /// Last frame of animation
        /// </summary>
        protected readonly IJsmExpression _lastFrame;


        public BGANIME(IJsmExpression firstFrame, IJsmExpression lastFrame)
        {
            _firstFrame = firstFrame;
            _lastFrame = lastFrame;
        }

        public BGANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                lastFrame: stack.Pop(),
                firstFrame: stack.Pop())
        {
        }
        public abstract override string ToString();
    }
}
