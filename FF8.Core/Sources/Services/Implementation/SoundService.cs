using System;

namespace FF8.Core
{
    public sealed class SoundService : ISoundService
    {
        public Boolean IsSupported => true;

        public void PlaySound(Int32 fieldSoundIndex, Int32 pan, Int32 volume, Int32 channel)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(SoundService)}.{nameof(PlaySound)}({nameof(fieldSoundIndex)}: {fieldSoundIndex}, {nameof(pan)}: {pan}, {nameof(volume)}: {volume}, {nameof(channel)}: {channel})");
        }
    }
}