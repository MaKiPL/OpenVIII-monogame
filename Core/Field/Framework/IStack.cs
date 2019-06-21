using System;

namespace OpenVIII
{
    public interface IStack<T>
    {
        Int32 Count { get; }
        void Push(T item);
        T Pop();
    }
}