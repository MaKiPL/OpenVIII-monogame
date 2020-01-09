using System;

namespace OpenVIII.Fields
{
    public interface IJumpToInstruction : IJsmInstruction
    {
        Int32 Index { get; set; }
    }
}