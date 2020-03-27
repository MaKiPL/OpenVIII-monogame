namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class COLSYNC : JsmInstruction
    {
        #region Constructors

        public COLSYNC()
        {
        }

        public COLSYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.Wait))
                .Comment(nameof(COLSYNC));

        public override IAwaitable TestExecute(IServices services) => ServiceId.Rendering[services].Wait();

        public override string ToString() => $"{nameof(COLSYNC)}()";

        #endregion Methods
    }
}