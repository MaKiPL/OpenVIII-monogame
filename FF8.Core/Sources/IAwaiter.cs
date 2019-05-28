using System;
using System.Runtime.CompilerServices;

namespace FF8.Core
{
    public interface IAwaiter : INotifyCompletion
    {
        Boolean IsCompleted { get; }
        void GetResult();
    }
}