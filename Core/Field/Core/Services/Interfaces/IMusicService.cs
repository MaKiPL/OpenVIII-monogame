using System;

namespace OpenVIII.Fields
{
    public interface IMusicService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void ChangeBattleMusic(MusicId musicId);

        void ChangeMusicVolume(int volume, bool flag);

        void ChangeMusicVolume(int volume, bool flag, int transitionDuration);

        void LoadFieldMusic(MusicId musicId);

        void PlayFieldMusic();

        #endregion Methods
    }
}