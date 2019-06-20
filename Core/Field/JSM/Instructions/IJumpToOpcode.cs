using System;

namespace FF8
{
    public interface IJumpToOpcode : IJumpToInstruction
    {
        Int32 Offset { get; }
    }
}