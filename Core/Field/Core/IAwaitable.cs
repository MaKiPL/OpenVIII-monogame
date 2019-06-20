using System;

namespace OpenVIII
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }
}