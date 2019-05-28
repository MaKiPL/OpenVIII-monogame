using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Play an animation.
    /// 
    /// ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP
    /// R - Async (don't wait for the animation)
    /// C - Range (play frame range)
    /// KEEP - Freeze (don't return the base animation, freeze on the last frame)
    /// </summary>
    internal sealed class ANIME : JsmInstruction
    {
        private Int32 _animationId;

        public ANIME(Int32 animationId)
        {
            _animationId = animationId;
        }

        public ANIME(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId)
        {
        }

        public override String ToString()
        {
            return $"{nameof(ANIME)}({nameof(_animationId)}: {_animationId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Comment(nameof(ANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            // Sync call
            return ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, freeze: false);
        }
    }
}