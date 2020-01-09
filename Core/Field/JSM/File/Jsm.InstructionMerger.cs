using System;
using System.Collections.Generic;



namespace OpenVIII.Fields
{
    public static partial class Jsm
    {
        public static class InstructionMerger
        {
            public static List<JsmInstruction> Merge(List<JsmInstruction> instructions, HashSet<Int32> labelIndices)
            {
                List<JsmInstruction> result = new List<JsmInstruction>(instructions.Count);

                for (Int32 i = 0; i < instructions.Count; i++)
                {
                    JsmInstruction instruction = instructions[i];
                    result.Add(instruction);
                    if (labelIndices.Contains(i))
                        continue;

                    if (!(instructions[i] is JPF newJpf))
                        continue;

                    if (i + 1 < instructions.Count && instructions[i + 1] is JMP nextJmp)
                        newJpf.Inverse(nextJmp);

                    if (!(result[result.Count - 2] is JPF oldJpf))
                        continue;

                    if (oldJpf.Index != newJpf.Index)
                        continue;

                    Int32 currentIndex = result.LastIndex();

                    oldJpf.Union(newJpf);
                    result.RemoveLast();
                    oldJpf.Index--;

                    for (Int32 k = 0; k < instructions.Count; k++)
                    {
                        if (instructions[k] is IJumpToInstruction jmp && jmp.Index >= currentIndex && jmp != oldJpf)
                            jmp.Index--;
                    }
                }

                return result;
            }
        }
    }
}