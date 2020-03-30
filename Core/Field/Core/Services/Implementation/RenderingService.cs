using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public sealed class RenderingService : IRenderingService
    {
        #region Properties

        public int BackgroundFPS { get; set; }
        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void AddScreenColor(Color Color) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColor)}({nameof(Color)}: {Color})");

        public void AddScreenColorTransition(Color Color, Color offset, int transitionDuration) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AddScreenColorTransition)}({nameof(Color)}: {Color}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");

        public void AnimateBackground(int firstFrame, int lastFrame) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(AnimateBackground)}({nameof(firstFrame)}: {firstFrame}, {nameof(lastFrame)}: {lastFrame})");

        public void DrawBackground() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(DrawBackground)}()");

        public void SubScreenColor(Color Color) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColor)}({nameof(Color)}: {Color})");

        public void SubScreenColorTransition(Color Color, Color offset, int transitionDuration) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(SubScreenColorTransition)}({nameof(Color)}: {Color}, {nameof(offset)}: {offset}, {nameof(transitionDuration)}: {transitionDuration})");

        public IAwaitable Wait()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(RenderingService)}.{nameof(Wait)}()");

            return DummyAwaitable.Instance;
        }

        #endregion Methods
    }
}