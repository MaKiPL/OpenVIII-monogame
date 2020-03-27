using System;

namespace OpenVIII.Fields
{
    public sealed class SoundService : ISoundService
    {
        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void PlaySound(int fieldSoundIndex, int pan, int volume, int channel)
        {
            AV.Sound.Play(fieldSoundIndex, volume, pan: pan);// what do i do with channel.
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(SoundService)}.{nameof(PlaySound)}({nameof(fieldSoundIndex)}: {fieldSoundIndex}, {nameof(pan)}: {pan}, {nameof(volume)}: {volume}, {nameof(channel)}: {channel})");
        }

        #endregion Methods
    }
}