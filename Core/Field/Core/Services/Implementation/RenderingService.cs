using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public sealed class RenderingService : IRenderingService
    {
        public Boolean IsSupported => true;

        public void AddScreenColor(Color Color)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColor)}({nameof(Color)}: {Color})");
        }

        public void SubScreenColor(Color Color)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColor)}({nameof(Color)}: {Color})");
        }

        public void AddScreenColorTransition(Color Color, Color offset, Int32 transitionDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColorTransition)}({nameof(Color)}: {Color}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");
        }

        public void SubScreenColorTransition(Color Color, Color offset, Int32 transitionDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColorTransition)}({nameof(Color)}: {Color}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");
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