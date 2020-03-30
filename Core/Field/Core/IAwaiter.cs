using System;
using System.Runtime.CompilerServices;

namespace OpenVIII.Fields
{
    public interface IAwaiter : INotifyCompletion
    {
        #region Properties

        bool IsCompleted { get; }

        #endregion Properties

        #region Methods

        void GetResult();

        #endregion Methods
    }
}