using System;

namespace FF8.Core
{
    public interface IMusicService
    {
        Boolean IsSupported { get; }

        void ChangeBattleMusic(MusicId musicId);
        void LoadFieldMusic(MusicId musicId);
        void PlayFieldMusic();
        void ChangeMusicVolume(Int32 volume, Boolean flag);
        void ChangeMusicVolume(Int32 volume, Boolean flag, Int32 transitionDuration);
    }
}