using System;

namespace OpenVIII.Fields
{
    public sealed class MessageService : IMessageService
    {
        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void Close(int channel) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Close)}({nameof(channel)}: {channel})");

        public void Show(int channel, int messageId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Show)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId})");

        public void Show(int channel, int messageId, int posX, int posY) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Show)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");

        public IAwaitable ShowDialog(int channel, int messageId, int posX, int posY)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowDialog)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");
            return DummyAwaitable.Instance;
        }

        public IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowQuestion)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(firstLine)}: {firstLine}, {nameof(lastLine)}: {lastLine}, {nameof(beginLine)}: {beginLine}, {nameof(cancelLine)}: {cancelLine})");
            return DummyAwaitable.Instance;
        }

        public IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine, int posX, int posY)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowQuestion)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(firstLine)}: {firstLine}, {nameof(lastLine)}: {lastLine}, {nameof(beginLine)}: {beginLine}, {nameof(cancelLine)}: {cancelLine}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");
            return DummyAwaitable.Instance;
        }

        #endregion Methods
    }
}