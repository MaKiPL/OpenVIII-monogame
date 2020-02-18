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
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/031_RANIME"/>
    public sealed class RANIME : Abstract.ANIME
    {
        public RANIME(int animationId) : base(animationId)
        {
        }

        public RANIME(int animationId, IStack<IJsmExpression> stack) : base(animationId, stack)
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