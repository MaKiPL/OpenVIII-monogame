using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Pop up a message window and pauses script execution until the player dismisses the window. 
    /// </summary>
    internal sealed class AMESW : JsmInstruction
    {
        private IJsmExpression _channel;
        private IJsmExpression _messageId;
        private IJsmExpression _posX;
        private IJsmExpression _posY;

        public AMESW(IJsmExpression channel, IJsmExpression messageId, IJsmExpression posX, IJsmExpression posY)
        {
            _channel = channel;
            _messageId = messageId;
            _posX = posX;
            _posY = posY;
        }

        public AMESW(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                posY: stack.Pop(),
                posX: stack.Pop(),
                messageId: stack.Pop(),
                channel: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(AMESW)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_posX)}: {_posX}, {nameof(_posY)}: {_posY})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            if (_messageId is IConstExpression message)
                FormatHelper.FormatMonologue(sw, formatterContext.GetMessage(message.Int32()));

            sw.Format(formatterContext, services)
                .Await()
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.ShowDialog))
                .Argument("channel", _channel)
                .Argument("messageId", _messageId)
                .Argument("posX", _posX)
                .Argument("posY", _posY)
                .Comment(nameof(AMESW));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            return ServiceId.Message[services].ShowDialog(
                _channel.Int32(services),
                _messageId.Int32(services),
                _posX.Int32(services),
                _posY.Int32(services));
        }
    }
}