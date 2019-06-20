using System;


namespace OpenVIII
{
    public static partial class Jsm
    {
        public sealed class IndexedInstruction
        {
            public Int32 Index { get; private set; }
            public JsmInstruction Instruction { get; private set; }

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