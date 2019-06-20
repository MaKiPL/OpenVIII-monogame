using System;

namespace OpenVIII
{
    public interface IJumpToInstruction : IJsmInstruction
    {
        Int32 Index { get; set; }
    }
}