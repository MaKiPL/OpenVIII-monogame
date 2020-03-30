using System;

namespace OpenVIII.Fields
{
    public sealed class GlobalVariableService : IGlobalVariableService
    {
        #region Fields

        private readonly long[] _values = new long[8192];

        #endregion Fields

        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public T Get<T>(GlobalVariableId<T> id) where T : unmanaged
        {
            unsafe
            {
                fixed (long* ptr = &_values[id.VariableId])
                    return *(T*)ptr;
            }
        }

        public void Set<T>(GlobalVariableId<T> id, T value) where T : unmanaged
        {
            unsafe
            {
                fixed (long* ptr = &_values[id.VariableId])
                    *(T*)ptr = value;
            }
        }

        #endregion Methods
    }
}