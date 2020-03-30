using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public interface IRenderingService
    {
        #region Properties

        int BackgroundFPS { get; set; }
        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void AddScreenColor(Color Color);

        void AddScreenColorTransition(Color Color, Color offset, int transitionDuration);

        void AnimateBackground(int firstFrame, int lastFrame);

        void DrawBackground();

        void SubScreenColor(Color Color);

        void SubScreenColorTransition(Color Color, Color offset, int transitionDuration);

        IAwaitable Wait();

        #endregion Methods
    }
}