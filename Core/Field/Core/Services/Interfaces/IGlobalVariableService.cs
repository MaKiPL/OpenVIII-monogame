using System;

namespace OpenVIII.Fields
{
    public interface IGlobalVariableService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        T Get<T>(GlobalVariableId<T> id) where T : unmanaged;

        void Set<T>(GlobalVariableId<T> id, T value) where T : unmanaged;

        #endregion Methods
    }
}