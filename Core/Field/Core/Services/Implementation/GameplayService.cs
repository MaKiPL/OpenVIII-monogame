using System;

namespace OpenVIII.Fields
{
    public sealed class GameplayService : IGameplayService
    {
        #region Properties

        public bool IsRandomBattlesEnabled { get; set; }
        public bool IsSupported => true;

        public bool IsUserControlEnabled { get; set; }

        #endregion Properties

        #region Methods

        public void ResetAllData() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(GameplayService)}.{nameof(ResetAllData)}()");

        #endregion Methods
    }
}