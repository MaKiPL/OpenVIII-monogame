namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Popup a message window.
    /// This is usually used on lines to popup text when the player crosses a certain point on a screen.
    /// The size of the message window can be set with WINSIZE.
    /// </summary>
    internal sealed class MES : JsmInstruction
    {
        #region Fields

        private readonly int _channel;
        private readonly int _messageId;

        #endregion Fields

        #region Constructors

        public MES(int channel, int messageId)
        {
            _channel = channel;
            _messageId = messageId;
        }

        public MES(int parameter, IStack<IJsmExpression> stack)
            : this(
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
                .Comment(nameof(MES));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Message[services].Show(_channel, _messageId);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MES)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId})";

        #endregion Methods
    }
}