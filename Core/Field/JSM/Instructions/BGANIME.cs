using System;


namespace OpenVIII
{
    /// <summary>
    /// Animates a background object on the field. 
    /// </summary>
    internal sealed class BGANIME : JsmInstruction
    {
        private IJsmExpression _lastFrame;
        private IJsmExpression _firstFrame;

        public BGANIME(IJsmExpression lastFrame, IJsmExpression firstFrame)
        {
            _lastFrame = lastFrame;
            _firstFrame = firstFrame;
        }

        public BGANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                firstFrame: stack.Pop(),
                lastFrame: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGANIME)}({nameof(_lastFrame)}: {_lastFrame}, {nameof(_firstFrame)}: {_firstFrame})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.AnimateBackground))
                .Argument("lastFrame", _lastFrame)
                .Argument("firstFrame", _firstFrame)
                .Comment(nameof(BGANIME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Int32 firstFrame = _lastFrame.Int32(services);
            Int32 lastFrame = _firstFrame.Int32(services);
            ServiceId.Rendering[services].AnimateBackground(firstFrame, lastFrame);
            return DummyAwaitable.Instance;
        }
    }
}