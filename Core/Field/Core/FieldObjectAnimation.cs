using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectAnimation
    {
        public Int32 BaseAnimationId { get; private set; }
        public Int32 FirstFrame { get; set; }
        public Int32 LastFrame { get; set; }

        public Int32 FPS { get; set; }

        public void ChangeBaseAnimation(Int32 animationId, Int32 firstFrame, Int32 lastFrame)
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

        public IAwaitable Play(Int32 animationId, Boolean freeze)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Play)}({nameof(animationId)}: {animationId}, {nameof(freeze)}: {freeze})");

            return DummyAwaitable.Instance;
        }

        public IAwaitable Play(Int32 animationId, Int32 firstFrame, Int32 lastFrame, Boolean freeze)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Play)}({nameof(animationId)}: {animationId}, {nameof(firstFrame)}: {firstFrame}, {nameof(lastFrame)}: {lastFrame}, {nameof(freeze)}: {freeze})");

            return DummyAwaitable.Instance;
        }
    }
}