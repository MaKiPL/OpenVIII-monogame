using Microsoft.Xna.Framework;

namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DCOLSUB : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _b;
        private readonly IJsmExpression _g;
        private readonly IJsmExpression _r;

        #endregion Fields

        #region Constructors

        public DCOLSUB(IJsmExpression r, IJsmExpression g, IJsmExpression b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public DCOLSUB(int parameter, IStack<IJsmExpression> stack)
            : this(
                b: stack.Pop(),
                g: stack.Pop(),
                r: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.SubScreenColor))
                .Argument("r", _r)
                .Argument("g", _g)
                .Argument("b", _b)
                .Comment(nameof(DCOLSUB));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].SubScreenColor(
                new Color(
                    _r.Int32(services),
                    _g.Int32(services),
                    _b.Int32(services)));
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(DCOLSUB)}({nameof(_r)}: {_r}, {nameof(_g)}: {_g}, {nameof(_b)}: {_b})";

        #endregion Methods
    }
}