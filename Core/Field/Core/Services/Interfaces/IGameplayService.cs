using System;

namespace OpenVIII
{
    public interface IGameplayService
    {
        Boolean IsSupported { get; }

        Boolean IsUserControlEnabled { get; set; }
        Boolean IsRandomBattlesEnabled { get; set; }

        void ResetAllData();
    }
}