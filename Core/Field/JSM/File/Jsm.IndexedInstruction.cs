using OpenVIII.Fields.Scripts.Instructions;
using System;


namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public sealed class IndexedInstruction
        {
            public Int32 Index { get;  }
            public JsmInstruction Instruction { get;  }

            public IndexedInstruction(Int32 index, JsmInstruction instruction)
            {
                Index = index;
                Instruction = instruction;
            }

            public override String ToString()
            {
                return $"[Index: {Index}] {Instruction}";
            }
        }
    }
}