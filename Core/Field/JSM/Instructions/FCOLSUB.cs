using System;


namespace FF8
{
    internal sealed class FCOLSUB : JsmInstruction
    {
        private IJsmExpression _r1;
        private IJsmExpression _g1;
        private IJsmExpression _b1;
        private IJsmExpression _r2;
        private IJsmExpression _g2;
        private IJsmExpression _b2;
        private IJsmExpression _transitionDuration;

        public FCOLSUB(IJsmExpression r1, IJsmExpression g1, IJsmExpression b1, IJsmExpression r2, IJsmExpression g2, IJsmExpression b2, IJsmExpression transitionDuration)
        {
            _r1 = r1;
            _g1 = g1;
            _b1 = b1;
            _r2 = r2;
            _g2 = g2;
            _b2 = b2;
            _transitionDuration = transitionDuration;
        }

        public FCOLSUB(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                transitionDuration: stack.Pop(),
                b2: stack.Pop(),
                g2: stack.Pop(),
                r2: stack.Pop(),
                b1: stack.Pop(),
                g1: stack.Pop(),
                r1: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(FCOLSUB)}({nameof(_r1)}: {_r1}, {nameof(_g1)}: {_g1}, {nameof(_b1)}: {_b1}, {nameof(_r2)}: {_r2}, {nameof(_g2)}: {_g2}, {nameof(_b2)}: {_b2}, {nameof(_transitionDuration)}: {_transitionDuration})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.SubScreenColorTransition))
                .Argument("r1", _r1)
                .Argument("g1", _g1)
                .Argument("b1", _b1)
                .Argument("r2", _r2)
                .Argument("g2", _g2)
                .Argument("b2", _b2)
                .Argument("transitionDuration", _transitionDuration)
                .Comment(nameof(FCOLSUB));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].SubScreenColorTransition(
                new RGBColor(
                    _r1.Int32(services),
                    _g1.Int32(services),
                    _b1.Int32(services)),
                new RGBColor(
                    _r2.Int32(services),
                    _g2.Int32(services),
                    _b2.Int32(services)),
                _transitionDuration.Int32(services));

            return DummyAwaitable.Instance;
        }
    }
}