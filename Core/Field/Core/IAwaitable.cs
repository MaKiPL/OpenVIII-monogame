using System;

namespace FF8
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }
}