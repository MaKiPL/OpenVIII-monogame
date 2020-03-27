namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set animation speed. Sets the speed of this entity's animations.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0E7_ANIMESPEED"/>
    public sealed class ANIMESPEED : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Frames per second.
        /// </summary>
        private readonly int _fps;

        #endregion Fields

        #region Constructors

        public ANIMESPEED(int fps) => _fps = fps;

        public ANIMESPEED(int parameter, IStack<IJsmExpression> stack)
            : this(
                //Frame speed of 1 means 2 frames per second. So doubling makes it fps.
                fps: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32() * 2) // Native: FPS / 2
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Property(nameof(FieldObjectAnimation.FPS))
                .Assign(_fps)
                .Comment(nameof(ANIMESPEED));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].Engine.CurrentObject.Animation.FPS = _fps;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(ANIMESPEED)}({nameof(_fps)}: {_fps})";

        #endregion Methods
    }
}