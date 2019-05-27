using System;

namespace FF8.Framework
{
    public interface IStack<T>
    {
        Int32 Count { get; }
        void Push(T item);
        T Pop();
    }
}