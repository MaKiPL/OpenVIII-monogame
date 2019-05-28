using System;

namespace FF8.Core
{
    public interface IRenderingService
    {
        Boolean IsSupported { get; }
        
        void AddScreenColor(RGBColor rgbColor);
        void SubScreenColor(RGBColor rgbColor);

        void AddScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration);
        void SubScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration);
        IAwaitable Wait();
        
        Int32 BackgroundFPS { get; set; }
        void AnimateBackground(Int32 firstFrame, Int32 lastFrame);
        void DrawBackground();
    }
}