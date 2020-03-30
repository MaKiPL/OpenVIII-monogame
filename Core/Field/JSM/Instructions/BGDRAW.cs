namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class BGDRAW : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public BGDRAW(IJsmExpression arg0) => _arg0 = arg0;

        public BGDRAW(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.DrawBackground))
                .Comment(nameof(BGDRAW));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].DrawBackground();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(BGDRAW)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}