using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJumpToInstruction : IJsmInstruction
    {
        Int32 Index { get; set; }
    }
}