using System;


namespace FF8
{
    /// <summary>
    /// Play an animation.
    /// 
    /// ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP
    /// R - Async (don't wait for the animation)
    /// C - Range (play frame range)
    /// KEEP - Freeze (don't return the base animation, freeze on the last frame)
    /// </summary>
    internal sealed class ANIMEKEEP : JsmInstruction
    {
        private Int32 _animationId;

        public ANIMEKEEP(Int32 animationId)
        {
            _animationId = animationId;
        }

        public ANIMEKEEP(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId)
        {
        }

        public override String ToString()
        {
            return $"{nameof(ANIMEKEEP)}({nameof(_animationId)}: {_animationId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Comment(nameof(ANIMEKEEP));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            // Sync call
            return ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, freeze: true);
        }
    }
}