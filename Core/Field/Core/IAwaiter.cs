using System;
using System.Runtime.CompilerServices;

namespace OpenVIII.Fields
{
    public interface IAwaiter : INotifyCompletion
    {
        Boolean IsCompleted { get; }
        void GetResult();
    }
}