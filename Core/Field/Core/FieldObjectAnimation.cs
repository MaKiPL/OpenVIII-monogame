using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectAnimation
    {
        public int BaseAnimationId { get; set; }
        public int FirstFrame { get; set; }
        public int LastFrame { get; set; }

        public int FPS { get; set; }

        public void ChangeBaseAnimation(int animationId, int firstFrame, int lastFrame)
        {
            BaseAnimationId = animationId;
            FirstFrame = firstFrame;
            LastFrame = lastFrame;

            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(ChangeBaseAnimation)}({nameof(animationId)}: {animationId}, {nameof(firstFrame)}: {firstFrame}, {nameof(lastFrame)}: {lastFrame})");
        }

        public IAwaitable Wait()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Wait)}()");

            return DummyAwaitable.Instance;
        }

        public IAwaitable Play(int animationId, bool freeze)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Play)}({nameof(animationId)}: {animationId}, {nameof(freeze)}: {freeze})");

            return DummyAwaitable.Instance;
        }

        public IAwaitable Play(int animationId, int firstFrame, int lastFrame, bool freeze)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Play)}({nameof(animationId)}: {animationId}, {nameof(firstFrame)}: {firstFrame}, {nameof(lastFrame)}: {lastFrame}, {nameof(freeze)}: {freeze})");

            return DummyAwaitable.Instance;
        }
    }
}