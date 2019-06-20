using System;

namespace OpenVIII
{
    public sealed class MessageService : IMessageService
    {
        public Boolean IsSupported => true;

        public void Show(Int32 channel, Int32 messageId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Show)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId})");
        }

        public void Show(Int32 channel, Int32 messageId, Int32 posX, Int32 posY)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Show)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");
        }

        public void Close(Int32 channel)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(Close)}({nameof(channel)}: {channel})");
        }

        public IAwaitable ShowDialog(Int32 channel, Int32 messageId, Int32 posX, Int32 posY)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowDialog)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");
            return DummyAwaitable.Instance;
        }

        public IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowQuestion)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(firstLine)}: {firstLine}, {nameof(lastLine)}: {lastLine}, {nameof(beginLine)}: {beginLine}, {nameof(cancelLine)}: {cancelLine})");
            return DummyAwaitable.Instance;
        }

        public IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine, Int32 posX, Int32 posY)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MessageService)}.{nameof(ShowQuestion)}({nameof(channel)}: {channel}, {nameof(messageId)}: {messageId}, {nameof(firstLine)}: {firstLine}, {nameof(lastLine)}: {lastLine}, {nameof(beginLine)}: {beginLine}, {nameof(cancelLine)}: {cancelLine}, {nameof(posX)}: {posX}, {nameof(posY)}: {posY})");
            return DummyAwaitable.Instance;
        }
    }
}