using System;

namespace OpenVIII.Fields
{
    public interface IMessageService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void Close(int channel);

        void Show(int channel, int messageId);

        void Show(int channel, int messageId, int posX, int posY);

        IAwaitable ShowDialog(int channel, int messageId, int posX, int posY);

        IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine);

        IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine, int posX, int posY);

        #endregion Methods
    }
}