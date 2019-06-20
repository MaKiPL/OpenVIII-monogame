using System;
using System.Runtime.CompilerServices;

namespace FF8
{
    public interface IAwaiter : INotifyCompletion
    {
        Boolean IsCompleted { get; }
        void GetResult();
    }
}