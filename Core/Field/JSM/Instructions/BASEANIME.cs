using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Sets this entity's model to loop the given frames of this animation while it's idle.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/02C_BASEANIME"/>
    public sealed class BASEANIME : Abstract.ANIMELOOP
    {
        public BASEANIME(int animationId, int firstFrame, int lastFrame) : base(animationId, firstFrame, lastFrame)
        {
        }

        public BASEANIME(int animationId, IStack<IJsmExpression> stack) : base(animationId, stack)
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