using System;

namespace FF8
{
    public interface IJumpToInstruction : IJsmInstruction
    {
        Int32 Index { get; set; }
    }
}