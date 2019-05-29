using System;

namespace FF8
{
    /// <summary>
    /// Pauses this script for some number of frames. 
    /// </summary>
    internal sealed class WAIT : JsmInstruction
    {
        private IJsmExpression _frameNumber;

        public WAIT(IJsmExpression frameNumber)
        {
            _frameNumber = frameNumber;
        }

        public WAIT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameNumber: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(WAIT)}({nameof(_frameNumber)}: {_frameNumber})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .StaticType(nameof(IInteractionService))
                .Method(nameof(IInteractionService.Wait))
                .Argument("frameNumber", _frameNumber)
                .Comment(nameof(WAIT));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Int32 frameNumber = _frameNumber.Int32(services);
            return ServiceId.Interaction[services].Wait(frameNumber);
        }
    }
}