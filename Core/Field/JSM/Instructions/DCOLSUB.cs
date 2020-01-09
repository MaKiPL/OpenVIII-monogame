using Microsoft.Xna.Framework;
using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DCOLSUB : JsmInstruction
    {
        private readonly IJsmExpression _r;
        private readonly IJsmExpression _g;
        private readonly IJsmExpression _b;

        public DCOLSUB(IJsmExpression r, IJsmExpression g, IJsmExpression b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public DCOLSUB(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                b: stack.Pop(),
                g: stack.Pop(),
                r: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DCOLSUB)}({nameof(_r)}: {_r}, {nameof(_g)}: {_g}, {nameof(_b)}: {_b})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.SubScreenColor))
                .Argument("r", _r)
                .Argument("g", _g)
                .Argument("b", _b)
                .Comment(nameof(DCOLSUB));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].SubScreenColor(
                new Color(
                    _r.Int32(services),
                    _g.Int32(services),
                    _b.Int32(services)));
            return DummyAwaitable.Instance;
        }
    }
}