using System;

namespace FF8.Core
{
    public interface IGameplayService
    {
        Boolean IsSupported { get; }

        Boolean IsUserControlEnabled { get; set; }
        Boolean IsRandomBattlesEnabled { get; set; }

        void ResetAllData();
    }
}