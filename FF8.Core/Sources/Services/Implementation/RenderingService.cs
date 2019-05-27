using System;

namespace FF8.Core
{
    public sealed class RenderingService : IRenderingService
    {
        public Boolean IsSupported => true;

        public void AddScreenColor(RGBColor rgbColor)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColor)}({nameof(rgbColor)}: {rgbColor})");
        }

        public void SubScreenColor(RGBColor rgbColor)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColor)}({nameof(rgbColor)}: {rgbColor})");
        }

        public void AddScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColorTransition)}({nameof(rgbColor)}: {rgbColor}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");
        }

        public void SubScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColorTransition)}({nameof(rgbColor)}: {rgbColor}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");
        }

        public IAwaitable Wait()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(Wait)}()");

            return DummyAwaitable.Instance;
        }

        public Int32 BackgroundFPS { get; set; }

        public void AnimateBackground(Int32 firstFrame, Int32 lastFrame)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AnimateBackground)}({nameof(firstFrame)}: {firstFrame}, {nameof(lastFrame)}: {lastFrame})");
        }

        public void DrawBackground()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(DrawBackground)}()");
        }
    }
}