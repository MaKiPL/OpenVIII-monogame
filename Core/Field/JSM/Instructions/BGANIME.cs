using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Animates a background object on the field. 
    /// </summary>
    public sealed class BGANIME : Abstract.BGANIME
    {
        public BGANIME(IJsmExpression firstFrame, IJsmExpression lastFrame) : base(firstFrame, lastFrame)
        {
        }

        public BGANIME(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
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