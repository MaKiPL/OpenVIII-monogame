using System;

namespace OpenVIII.Fields
{
    public interface IJumpToOpcode : IJumpToInstruction
    {
        Int32 Offset { get; }
    }
}