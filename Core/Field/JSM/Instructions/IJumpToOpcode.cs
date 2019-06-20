using System;

namespace OpenVIII
{
    public interface IJumpToOpcode : IJumpToInstruction
    {
        Int32 Offset { get; }
    }
}