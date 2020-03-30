using Microsoft.Xna.Framework;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Pop up a message window until WINCLOSE or MESSYNC is called.
    /// </summary>
    public sealed class AMES : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Message Channel
        /// </summary>
        private readonly int _channel;

        /// <summary>
        /// Field message ID
        /// </summary>
        private readonly int _messageId;

        /// <summary>
        /// position of window
        /// </summary>
        private readonly Point _pos;

        #endregion Fields

        #region Constructors

        public AMES(int channel, int messageId, int posX, int posY)
        {
            _channel = channel;
            _messageId = messageId;
            (_pos.X, _pos.Y) = (posX, posY);
        }

        public AMES(int parameter, IStack<IJsmExpression> stack)
            : this(
                posY: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                posX: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                messageId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                channel: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

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

        public override string ToString() => $"{nameof(AMES)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_pos)}: {_pos})";

        #endregion Methods
    }
}