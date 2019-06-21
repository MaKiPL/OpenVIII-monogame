using System;

namespace OpenVIII
{
    public sealed class FieldObjectInteraction
    {
        public Boolean SoundFootsteps { get; set; }
        public Boolean IsTalkScriptActive { get; set; }
        public Boolean IsPushScriptActive { get; set; }
        public Int32 MovementSpeed { get; set; }

        public void Move(Coords3D coords, Int32 unknown)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Move)}({nameof(coords)}: {coords}, {nameof(unknown)}: {unknown})");
        }
    }
}