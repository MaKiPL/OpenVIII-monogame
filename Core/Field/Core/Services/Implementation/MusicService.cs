using System;

namespace OpenVIII.Fields
{
    public sealed class MusicService : IMusicService
    {
        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void ChangeBattleMusic(MusicId musicId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeBattleMusic)}({nameof(musicId)}: {musicId})");

        public void ChangeMusicVolume(int volume, bool flag) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeMusicVolume)}({nameof(volume)}: {volume}, {nameof(flag)}: {flag})");

        public void ChangeMusicVolume(int volume, bool flag, int transitionDuration) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeMusicVolume)}({nameof(volume)}: {volume}, {nameof(flag)}: {flag}, {nameof(transitionDuration)}: {transitionDuration})");

        public void LoadFieldMusic(MusicId musicId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(LoadFieldMusic)}({nameof(musicId)}: {musicId})");

        public void PlayFieldMusic() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(PlayFieldMusic)}()");

        #endregion Methods
    }
}