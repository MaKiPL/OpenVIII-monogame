using System;

namespace OpenVIII.Fields
{
    public interface IInteractionService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Indexers

        int this[ScriptResultId id] { get; set; }

        #endregion Indexers

        #region Methods

        IAwaitable Wait(int frameNumber);

        #endregion Methods
    }
}