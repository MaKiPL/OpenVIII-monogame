using System;

namespace FF8
{
    public interface IGameplayService
    {
        Boolean IsSupported { get; }

        Boolean IsUserControlEnabled { get; set; }
        Boolean IsRandomBattlesEnabled { get; set; }

        void ResetAllData();
    }
}