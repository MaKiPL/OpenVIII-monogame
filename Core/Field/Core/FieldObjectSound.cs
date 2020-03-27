using System;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectInteraction
    {
        #region Properties

        public bool IsPushScriptActive { get; set; }
        public bool IsTalkScriptActive { get; set; }
        public int MovementSpeed { get; set; }
        public bool SoundFootsteps { get; set; }

        #endregion Properties

        #region Methods

        public void Move(Coords3D coords, int unknown) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldObjectAnimation)}.{nameof(Move)}({nameof(coords)}: {coords}, {nameof(unknown)}: {unknown})");

        #endregion Methods
    }
}