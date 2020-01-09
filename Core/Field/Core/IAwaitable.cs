using System;

namespace OpenVIII.Fields
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }
}