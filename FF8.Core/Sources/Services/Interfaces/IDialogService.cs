using System;
using System.Threading.Tasks;

namespace FF8.Core
{
    public interface IMessageService
    {
        Boolean IsSupported { get; }

        void Show(Int32 channel, Int32 messageId);
        void Show(Int32 channel, Int32 messageId, Int32 posX, Int32 posY);
        void Close(Int32 channel);

        IAwaitable ShowDialog(Int32 channel, Int32 messageId, Int32 posX, Int32 posY);
        IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine);
        IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine, Int32 posX, Int32 posY);
    }
}