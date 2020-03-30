using System;

namespace OpenVIII.Fields
{
    public interface IStack<T>
    {
        int Count { get; }
        void Push(T item);
        T Pop();
    }
}