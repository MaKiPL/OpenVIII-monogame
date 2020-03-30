using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public sealed class LabelBuilder
        {
            #region Fields

            private readonly ushort _opcodeCount;
            private readonly Dictionary<int, List<IJumpToOpcode>> _targets = new Dictionary<int, List<IJumpToOpcode>>();
            private Dictionary<int, IndexedInstruction> _candidates = new Dictionary<int, IndexedInstruction>();

            #endregion Fields

            #region Constructors

            public LabelBuilder(ushort opcodeCount) => _opcodeCount = opcodeCount;

            #endregion Constructors

            #region Methods

            public HashSet<int> Commit()
            {
                var result = new HashSet<int>();

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

            public void TraceInstruction(int position, int label, IndexedInstruction instruction)
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

            #endregion Methods
        }

        #endregion Classes
    }
}