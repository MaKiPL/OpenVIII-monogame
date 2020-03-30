using System;

namespace OpenVIII.Fields
{
    public interface ISoundService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void PlaySound(int fieldSoundIndex, int pan, int volume, int channel);

        #endregion Methods
    }
}