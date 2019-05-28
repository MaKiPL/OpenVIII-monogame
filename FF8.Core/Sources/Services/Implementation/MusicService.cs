using System;

namespace FF8.Core
{
    public sealed class MusicService : IMusicService
    {
        public Boolean IsSupported => true;

        public void ChangeBattleMusic(MusicId musicId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeBattleMusic)}({nameof(musicId)}: {musicId})");
        }

        public void LoadFieldMusic(MusicId musicId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(LoadFieldMusic)}({nameof(musicId)}: {musicId})");
        }

        public void PlayFieldMusic()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(PlayFieldMusic)}()");
        }

        public void ChangeMusicVolume(Int32 volume, Boolean flag)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeMusicVolume)}({nameof(volume)}: {volume}, {nameof(flag)}: {flag})");
        }

        public void ChangeMusicVolume(Int32 volume, Boolean flag, Int32 transitionDuration)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MusicService)}.{nameof(ChangeMusicVolume)}({nameof(volume)}: {volume}, {nameof(flag)}: {flag}, {nameof(transitionDuration)}: {transitionDuration})");
        }
    }
}