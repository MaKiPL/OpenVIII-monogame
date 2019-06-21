using System;

namespace OpenVIII
{
    public interface ISoundService
    {
        Boolean IsSupported { get; }

        void PlaySound(Int32 fieldSoundIndex, Int32 pan, Int32 volume, Int32 channel);
    }
}