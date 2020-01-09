using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Play an animation.
    /// 
    /// ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP
    /// R - Async (don't wait for the animation)
    /// C - Range (play frame range)
    /// KEEP - Freeze (don't return the base animation, freeze on the last frame)
    /// </summary>
    internal sealed class RANIME : JsmInstruction
    {
        private Int32 _animationId;

        public RANIME(Int32 animationId)
        {
            _animationId = animationId;
        }

        public RANIME(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId)
        {
        }

        public override String ToString()
        {
            return $"{nameof(RANIME)}({nameof(_animationId)}: {_animationId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Comment(nameof(RANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, freeze: false);

            // Async call
            return DummyAwaitable.Instance;
        }
    }
}