using System;


namespace OpenVIII
{
    /// <summary>
    /// Play an animation.
    /// 
    /// ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP
    /// R - Async (don't wait for the animation)
    /// C - Range (play frame range)
    /// KEEP - Freeze (don't return the base animation, freeze on the last frame)
    /// </summary>
    internal sealed class RCANIME : JsmInstruction
    {
        private Int32 _animationId;
        private Int32 _lastFrame;
        private Int32 _firstFrame;

        public RCANIME(Int32 animationId, Int32 lastFrame, Int32 firstFrame)
        {
            _animationId = animationId;
            _lastFrame = lastFrame;
            _firstFrame = firstFrame;
        }

        public RCANIME(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId,
                firstFrame: ((IConstExpression)stack.Pop()).Int32(),
                lastFrame: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(RCANIME)}({nameof(_animationId)}: {_animationId}, {nameof(_lastFrame)}: {_lastFrame}, {nameof(_firstFrame)}: {_firstFrame})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Argument("firstFrame", _firstFrame)
                .Argument("lastFrame", _lastFrame)
                .Comment(nameof(RCANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, _firstFrame, _lastFrame, freeze: false);

            // Async call
            return DummyAwaitable.Instance;
        }
    }
}