using System;


namespace OpenVIII.Fields.Scripts.Instructions
{

    /// <summary>
    /// <para>Play an animation.</para>
    /// <para>ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP</para>
    /// <para>R - Async (don't wait for the animation)</para>
    /// <para>C - Range (play frame range)</para>
    /// <para>KEEP - Freeze (don't return the base animation, freeze on the last frame)</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/02F_CANIME"/>
    public sealed class CANIME : Abstract.ANIMELOOP
    {
        public CANIME(int animationId, int firstFrame, int lastFrame) : base(animationId, firstFrame, lastFrame)
        {
        }

        public CANIME(int animationId, IStack<IJsmExpression> stack) : base(animationId, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(CANIME)}({nameof(_animationId)}: {_animationId}, {nameof(_lastFrame)}: {_lastFrame}, {nameof(_firstFrame)}: {_firstFrame})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Argument("firstFrame", _firstFrame)
                .Argument("lastFrame", _lastFrame)
                .Comment(nameof(CANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            // Sync call
            return ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, _firstFrame, _lastFrame, freeze: false);
        }
    }
}