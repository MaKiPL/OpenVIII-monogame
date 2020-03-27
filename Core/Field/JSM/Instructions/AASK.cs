using Microsoft.Xna.Framework;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Opens a field message window and lets player choose a single line. AASK saves the chosen line index (first option is always 0) into a temp variable which you can retrieve with PSHI_L 0.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/06F_AASK"/>
    public sealed class AASK : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Line of default option
        /// </summary>
        private readonly IJsmExpression _beginLine;

        /// <summary>
        /// Line of cancel option
        /// </summary>
        private readonly IJsmExpression _cancelLine;

        /// <summary>
        /// Message Channel
        /// </summary>
        private readonly IJsmExpression _channel;

        /// <summary>
        /// Line of first option
        /// </summary>
        private readonly IJsmExpression _firstLine;

        /// <summary>
        /// Line of last option
        /// </summary>
        private readonly IJsmExpression _lastLine;

        /// <summary>
        /// Field message ID
        /// </summary>
        private readonly IJsmExpression _messageId;

        /// <summary>
        /// position of window
        /// </summary>
        private readonly Point _pos;

        #endregion Fields

        #region Constructors

        public AASK(IJsmExpression channel, IJsmExpression messageId, IJsmExpression firstLine, IJsmExpression lastLine, IJsmExpression beginLine, IJsmExpression cancelLine, int posX, int posY)
        {
            _channel = channel;
            _messageId = messageId;
            _firstLine = firstLine;
            _lastLine = lastLine;
            _beginLine = beginLine;
            _cancelLine = cancelLine;
            (_pos.X, _pos.Y) = (posX, posY);
        }

        public AASK(int parameter, IStack<IJsmExpression> stack)
            : this(
                posY: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                posX: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                cancelLine: stack.Pop(),
                beginLine: stack.Pop(),
                lastLine: stack.Pop(),
                firstLine: stack.Pop(),
                messageId: stack.Pop(),
                channel: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

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
                .Argument("posX", _pos.X)
                .Argument("posY", _pos.Y)
                .Comment(nameof(AASK));
        }

        public override IAwaitable TestExecute(IServices services) => ServiceId.Message[services].ShowQuestion(
                _channel.Int32(services),
                _messageId.Int32(services),
                _firstLine.Int32(services),
                _lastLine.Int32(services),
                _beginLine.Int32(services),
                _cancelLine.Int32(services),
                _pos.X,//do we need to use the services code for these? they don't do it in AMES
                _pos.Y);

        public override string ToString() => $"{nameof(AASK)}({nameof(_channel)}: {_channel}, {nameof(_messageId)}: {_messageId}, {nameof(_firstLine)}: {_firstLine}, {nameof(_lastLine)}: {_lastLine}, {nameof(_beginLine)}: {_beginLine}, {nameof(_cancelLine)}: {_cancelLine}, {nameof(_pos)}: {_pos})";

        #endregion Methods
    }
}