namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Background Animation Speed
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/09B_BGANIMESPEED&action=edit&redlink=1"/>
    public sealed class BGANIMESPEED : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// frame per half second.
        /// </summary>
        private readonly IJsmExpression _halfFps;

        #endregion Fields

        #region Constructors

        public BGANIMESPEED(IJsmExpression halfFps) => _halfFps = halfFps;

        public BGANIMESPEED(int parameter, IStack<IJsmExpression> stack)
            : this(
                halfFps: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Property(nameof(IRenderingService.BackgroundFPS))
                .Assign(_halfFps)
                .Comment(nameof(BGANIMESPEED));

        public override IAwaitable TestExecute(IServices services)
        {
            var fps = _halfFps.Int32(services) * 2;
            ServiceId.Rendering[services].BackgroundFPS = fps;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(BGANIMESPEED)}({nameof(_halfFps)}: {_halfFps})";

        #endregion Methods
    }
}