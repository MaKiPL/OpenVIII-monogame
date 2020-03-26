using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;


namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public sealed class LabelBuilder
        {
            private readonly Dictionary<Int32, List<IJumpToOpcode>> _targets = new Dictionary<Int32, List<IJumpToOpcode>>();
            private Dictionary<Int32, IndexedInstruction> _candidates = new Dictionary<Int32, IndexedInstruction>();

            private readonly UInt16 _opcodeCount;

            public LabelBuilder(UInt16 opcodeCount)
            {
                _opcodeCount = opcodeCount;
            }

            public void TraceInstruction(Int32 position, Int32 label, IndexedInstruction instruction)
            {
                _candidates.Add(label, instruction);

                if (!(instruction.Instruction is IJumpToOpcode jump))
                    return;

                var target = position + jump.Offset;
                if (target < 0)
                    throw new InvalidProgramException($"Trying to jump out of script ({position} -> {target}). The field \"test3.jsm\" isn't supported.");

                if (target >= _opcodeCount)
                {
                    if (target == 74) // escouse1.jsm (Lunar Gate - Concourse)
                        target = _opcodeCount - 1;
                    else
                        throw new InvalidProgramException($"Trying to jump out of script ({position} -> {target}).");
                }

                if (!_targets.TryGetValue(target, out var list))
                {
                    list = new List<IJumpToOpcode>();
                    _targets.Add(target, list);
                }

                list.Add(jump);
            }

            public HashSet<Int32> Commit()
            {
                var result = new HashSet<Int32>();

                foreach (var pair in _targets)
                {
                    var offset = pair.Key;
                    if (!_candidates.TryGetValue(offset, out var target))
                        throw new InvalidProgramException($"Invalid jump target: {pair.Key}");

                    foreach (var jump in pair.Value)
                        jump.Index = target.Index;

                    result.Add(target.Index);
                }

                return result;
            }
        }
    }
}