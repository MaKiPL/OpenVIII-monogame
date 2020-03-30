namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Pop up a message window and wait for the player to hit "ok." Unlike AMESW, RAMESW will let the script continue running.
    /// </summary>
    internal sealed class RAMESW : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _channel;
        private readonly IJsmExpression _messageId;
        private readonly IJsmExpression _posX;
        private readonly IJsmExpression _posY;

        #endregion Fields

        #region Constructors

        public RAMESW(IJsmExpression channel, IJsmExpression messageId, IJsmExpression posX, IJsmExpression posY)
        {
            _channel = channel;
            _messageId = messageId;
            _posX = posX;
            _posY = posY;
        }

        public RAMESW(int parameter, IStack<IJsmExpression> stack)
            : this(
                posY: stack.Pop(),
                posX: stack.Pop(),
                messageId: stack.Pop(),
                channel: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            if (_messageId is IConstExpression message)
                FormatHelper.FormatMonologue(sw, formatterContext.GetMessage(message.Int32()));

            sw.Format(formatterContext, services)
                .StaticType(nameof(IMessageService))
                .Method(nameof(IMessageService.ShowDialog))
                .Argument("channel", _channel)
                .Argument("messageId", _messageId)
                .Argument("posX", _posX)
                .Argument("posY", _posY)
                .Comment(nameof(RAMESW));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Message[services].ShowDialog(
                _channel.Int32(services),
                _messageId.Int32(services),
                _posX.Int32(services),
                _posY.Int32(services));
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(RAMESW)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_posX)}: {_posX}, {nameof(_posY)}: {_posY})";

        #endregion Methods
    }
}