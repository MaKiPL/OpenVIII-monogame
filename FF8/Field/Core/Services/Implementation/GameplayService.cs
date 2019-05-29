using System;

namespace FF8
{
    public sealed class GameplayService : IGameplayService
    {
        public Boolean IsSupported => true;

        public Boolean IsUserControlEnabled { get; set; }
        public Boolean IsRandomBattlesEnabled { get; set; }

        public void ResetAllData()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(GameplayService)}.{nameof(ResetAllData)}()");
        }
    }
}