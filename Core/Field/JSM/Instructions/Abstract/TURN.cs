using System;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class TURN : JsmInstruction
    {
        /// <summary>
        /// Duration of turn (frames)
        /// </summary>
        protected IJsmExpression _frames;
        /// <summary>
        /// Angle
        /// </summary>
        protected IJsmExpression _angle;

        public TURN(IJsmExpression frames, IJsmExpression angle)
        {
            _frames = frames;
            _angle = angle;
        }

        public TURN(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                angle: stack.Pop(),
                frames: stack.Pop())
        {
        }

        public abstract override string ToString();
    }
}
