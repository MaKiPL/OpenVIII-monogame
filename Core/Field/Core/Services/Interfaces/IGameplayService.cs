using System;

namespace OpenVIII.Fields
{
    public interface IGameplayService
    {
        #region Properties

        bool IsRandomBattlesEnabled { get; set; }
        bool IsSupported { get; }

        bool IsUserControlEnabled { get; set; }

        #endregion Properties

        #region Methods

        void ResetAllData();

        #endregion Methods
    }
}