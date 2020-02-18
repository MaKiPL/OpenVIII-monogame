using Microsoft.Xna.Framework;
using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Pop up a message window until WINCLOSE or MESSYNC is called. 
    /// </summary>
    public sealed class AMES : JsmInstruction
    {
        /// <summary>
        /// Message Channel
        /// </summary>
        private readonly Int32 _channel;
        /// <summary>
        /// Field message ID
        /// </summary>
        private readonly Int32 _messageId;
        /// <summary>
        /// position of window
        /// </summary>
        private readonly Point _pos;

        public AMES(Int32 channel, Int32 messageId, Int32 posX, Int32 posY)
        {
            _channel = channel;
            _messageId = messageId;
            (_pos.X, _pos.Y) = (posX, posY);
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
            return $"{nameof(AMES)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_pos)}: {_pos})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            FormatHelper.FormatMonologue(sw, formatterContext.GetMessage(_messageId));

            sw.Format(formatterContext, services)
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.Show))
                .Argument("channel", _channel)
                .Argument("messageId", _messageId)
                .Argument("posX", _pos.X)
                .Argument("posY", _pos.Y)
                .Comment(nameof(AMES));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Message[services].Show(_channel, _messageId, _pos.X, _pos.Y);
            return DummyAwaitable.Instance;
        }
    }
}