namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Play an animation.</para>
    /// <para>ANIME, CANIME, RANIME, RCANIME, ANIMEKEEP, CANIMEKEEP, RANIMEKEEP, RCANIMEKEEP</para>
    /// <para>R - Async (don't wait for the animation)</para>
    /// <para>C - Range (play frame range)</para>
    /// <para>KEEP - Freeze (don't return the base animation, freeze on the last frame)</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/034_RCANIMEKEEP"/>
    public sealed class RCANIMEKEEP : Abstract.ANIMELOOP
    {
        #region Constructors

        public RCANIMEKEEP(int animationId, int firstFrame, int lastFrame) : base(animationId, firstFrame, lastFrame)
        {
        }

        public RCANIMEKEEP(int animationId, IStack<IJsmExpression> stack) : base(animationId, stack)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Method(nameof(FieldObjectAnimation.Play))
                .Argument("animationId", _animationId)
                .Argument("firstFrame", _firstFrame)
                .Argument("lastFrame", _lastFrame)
                .Comment(nameof(RCANIMEKEEP));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].Engine.CurrentObject.Animation.Play(_animationId, _firstFrame, _lastFrame, freeze: true);

            // Async call
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(RCANIMEKEEP)}({nameof(_animationId)}: {_animationId}, {nameof(_lastFrame)}: {_lastFrame}, {nameof(_firstFrame)}: {_firstFrame})";

        #endregion Methods
    }
}