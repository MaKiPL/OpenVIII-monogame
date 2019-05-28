using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    internal sealed class DCOLADD : JsmInstruction
    {
        private readonly IJsmExpression _r;
        private readonly IJsmExpression _g;
        private readonly IJsmExpression _b;

        public DCOLADD(IJsmExpression r, IJsmExpression g, IJsmExpression b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public DCOLADD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                b: stack.Pop(),
                g: stack.Pop(),
                r: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DCOLADD)}({nameof(_r)}: {_r}, {nameof(_g)}: {_g}, {nameof(_b)}: {_b})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.AddScreenColor))
                .Argument("r", _r)
                .Argument("g", _g)
                .Argument("b", _b)
                .Comment(nameof(DCOLADD));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].AddScreenColor(
                new RGBColor(
                    _r.Int32(services),
                    _g.Int32(services),
                    _b.Int32(services)));
            return DummyAwaitable.Instance;
        }
    }
}