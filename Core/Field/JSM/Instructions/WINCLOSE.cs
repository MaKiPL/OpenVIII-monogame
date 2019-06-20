using System;


namespace FF8
{
    /// <summary>
    /// Close the last window created by AMES.
    /// I haven't tried this for other types of windows... 
    /// </summary>
    internal sealed class WINCLOSE : JsmInstruction
    {
        private Int32 _channel;

        public WINCLOSE(Int32 channel)
        {
            _channel = channel;
        }

        public WINCLOSE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                channel: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(WINCLOSE)}({nameof(_channel)}: {_channel})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.Close))
                .Argument("channel", _channel)
                .Comment(nameof(WINCLOSE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Message[services].Close(_channel);
            return DummyAwaitable.Instance;
        }
    }
}