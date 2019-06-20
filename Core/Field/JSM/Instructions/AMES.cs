using System;


namespace FF8
{
    /// <summary>
    /// Pop up a message window until WINCLOSE or MESSYNC is called. 
    /// </summary>
    internal sealed class AMES : JsmInstruction
    {
        private readonly Int32 _channel;
        private readonly Int32 _messageId;
        private readonly Int32 _posX;
        private readonly Int32 _posY;

        public AMES(Int32 channel, Int32 messageId, Int32 posX, Int32 posY)
        {
            _channel = channel;
            _messageId = messageId;
            _posX = posX;
            _posY = posY;
        }

        public AMES(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                posY: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                posX: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                messageId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                channel: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(AMES)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_posX)}: {_posX}, {nameof(_posY)}: {_posY})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            FormatHelper.FormatMonologue(sw, formatterContext.GetMessage(_messageId));

            sw.Format(formatterContext, services)
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.Show))
                .Argument("channel", _channel)
                .Argument("messageId", _messageId)
                .Argument("posX", _posX)
                .Argument("posY", _posY)
                .Comment(nameof(AMES));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Message[services].Show(_channel, _messageId, _posX, _posY);
            return DummyAwaitable.Instance;
        }
    }
}