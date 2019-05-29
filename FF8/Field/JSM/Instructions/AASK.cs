using System;


namespace FF8
{
    /// <summary>
    /// Opens a field message window and lets player choose a single line. AASK saves the chosen line index (first option is always 0) into a temp variable which you can retrieve with PSHI_L 0. 
    /// </summary>
    internal sealed class AASK : JsmInstruction
    {
        private IJsmExpression _channel;
        private IJsmExpression _messageId;
        private IJsmExpression _firstLine;
        private IJsmExpression _lastLine;
        private IJsmExpression _beginLine;
        private IJsmExpression _cancelLine;
        private IJsmExpression _posX;
        private IJsmExpression _posY;

        public AASK(IJsmExpression channel, IJsmExpression messageId, IJsmExpression firstLine, IJsmExpression lastLine, IJsmExpression beginLine, IJsmExpression cancelLine, IJsmExpression posX, IJsmExpression posY)
        {
            _channel = channel;
            _messageId = messageId;
            _firstLine = firstLine;
            _lastLine = lastLine;
            _beginLine = beginLine;
            _cancelLine = cancelLine;
            _posX = posX;
            _posY = posY;
        }

        public AASK(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                posY: stack.Pop(),
                posX: stack.Pop(),
                cancelLine: stack.Pop(),
                beginLine: stack.Pop(),
                lastLine: stack.Pop(),
                firstLine: stack.Pop(),
                messageId: stack.Pop(),
                channel: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(AASK)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_firstLine)}: {_firstLine}, {nameof(_lastLine)}: {_lastLine}, {nameof(_beginLine)}: {_beginLine}, {nameof(_cancelLine)}: {_cancelLine}, {nameof(_posX)}: {_posX}, {nameof(_posY)}: {_posY})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            if (_messageId is IConstExpression message)
                FormatHelper.FormatAnswers(sw, formatterContext.GetMessage(message.Int32()), _firstLine, _lastLine, _beginLine, _cancelLine);

            sw.Format(formatterContext, services)
                .Await()
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.ShowDialog))
                .Argument("channel", _channel)
                .Argument("messageId", _messageId)
                .Argument("firstLine", _firstLine)
                .Argument("lastLine", _lastLine)
                .Argument("beginLine", _beginLine)
                .Argument("cancelLine", _cancelLine)
                .Argument("posX", _posX)
                .Argument("posY", _posY)
                .Comment(nameof(AASK));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            return ServiceId.Message[services].ShowQuestion(
                _channel.Int32(services),
                _messageId.Int32(services),
                _firstLine.Int32(services),
                _lastLine.Int32(services),
                _beginLine.Int32(services),
                _cancelLine.Int32(services),
                _posX.Int32(services),
                _posY.Int32(services));
        }
    }
}