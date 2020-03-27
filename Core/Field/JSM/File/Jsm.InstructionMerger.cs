using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static class InstructionMerger
        {
            #region Methods

            public static List<JsmInstruction> Merge(List<JsmInstruction> instructions, HashSet<int> labelIndices)
            {
                var result = new List<JsmInstruction>(instructions.Count);

                for (var i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
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

                    var currentIndex = result.LastIndex();

                    oldJpf.Union(newJpf);
                    result.RemoveLast();
                    oldJpf.Index--;

                    for (var k = 0; k < instructions.Count; k++)
                    {
                        if (instructions[k] is IJumpToInstruction jmp && jmp.Index >= currentIndex && jmp != oldJpf)
                            jmp.Index--;
                    }
                }

                return result;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}