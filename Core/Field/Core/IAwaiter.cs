using System;
using System.Runtime.CompilerServices;

namespace OpenVIII
{
    public interface IAwaiter : INotifyCompletion
    {
        Boolean IsCompleted { get; }
        void GetResult();
    }
}