using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public interface IRenderingService
    {
        Boolean IsSupported { get; }
        
        void AddScreenColor(Color rgbColor);
        void SubScreenColor(Color rgbColor);

        void AddScreenColorTransition(Color rgbColor, Color offset, Int32 transitionDuration);
        void SubScreenColorTransition(Color rgbColor, Color offset, Int32 transitionDuration);
        IAwaitable Wait();
        
        Int32 BackgroundFPS { get; set; }
        void AnimateBackground(Int32 firstFrame, Int32 lastFrame);
        void DrawBackground();
    }
}