using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public interface IRenderingService
    {
        Boolean IsSupported { get; }
        
        void AddScreenColor(Color Color);
        void SubScreenColor(Color Color);

        void AddScreenColorTransition(Color Color, Color offset, Int32 transitionDuration);
        void SubScreenColorTransition(Color Color, Color offset, Int32 transitionDuration);
        IAwaitable Wait();
        
        Int32 BackgroundFPS { get; set; }
        void AnimateBackground(Int32 firstFrame, Int32 lastFrame);
        void DrawBackground();
    }
}