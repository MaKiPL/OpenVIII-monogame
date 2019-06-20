using System;


namespace FF8
{
    internal sealed class BASEANIME : JsmInstruction
    {
        private Int32 _animationId;
        private Int32 _lastFrame;
        private Int32 _firstFrame;

        public BASEANIME(Int32 animationId, Int32 lastFrame, Int32 firstFrame)
        {
            _animationId = animationId;
            _lastFrame = lastFrame;
            _firstFrame = firstFrame;
        }

        public BASEANIME(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId,
                firstFrame: ((IConstExpression)stack.Pop()).Int32(),
                lastFrame: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BASEANIME)}({nameof(_animationId)}: {_animationId}, {nameof(_lastFrame)}: {_lastFrame}, {nameof(_firstFrame)}: {_firstFrame})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.ChangeBaseAnimation))
                .Argument("animationId", _animationId)
                .Argument("firstFrame", _firstFrame)
                .Argument("lastFrame", _lastFrame)
                .Comment(nameof(BASEANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Animation.ChangeBaseAnimation(_animationId, _firstFrame, _lastFrame);
            return DummyAwaitable.Instance;
        }
    }
}