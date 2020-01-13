using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJumpToOpcode : IJumpToInstruction
    {
        Int32 Offset { get; }
    }
}