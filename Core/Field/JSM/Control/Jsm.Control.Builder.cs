using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Control
        {
            #region Classes

            public sealed class Builder
            {
                #region Fields

                private readonly List<JsmInstruction> _instructions;

                private readonly ProcessedJumps _processed = new ProcessedJumps();

                private readonly List<IJsmControl> _result = new List<IJsmControl>();

                private JPF _begin;

                private int _index;

                #endregion Fields

                #region Constructors

                private Builder(List<JsmInstruction> instructions) => _instructions = instructions;

                #endregion Constructors

                #region Methods

                public static IReadOnlyList<IJsmControl> Build(List<JsmInstruction> instructions) => new Builder(instructions).Make();

                public IReadOnlyList<IJsmControl> Make()
                {
                    for (_index = 0; _index < _instructions.Count; _index++)
                    {
                        if (TryMakeGotoOrSkip())
                            continue;

                        if (TryMakeWhile())
                            continue;

                        if (TryMakeIf())
                            continue;

                        throw new InvalidProgramException($"Cannot recognize the logical block: {_begin}");
                    }

                    return _result;
                }

                private void AddIfElseBranches(If control, JPF jpf, JMP jmp)
                {
                    var endOfBlock = jmp.Index;

                    while (true)
                    {
                        var newJpfIndex = jpf.Index;
                        if (!(_instructions[newJpfIndex] is JPF newJpf) || newJpf.Index > endOfBlock)
                        {
                            control.AddElse(jpf.Index, endOfBlock);
                            return;
                        }

                        if (!(_instructions[newJpf.Index - 1] is JMP newJmp))
                        {
                            if (newJpf.Index == endOfBlock)
                            {
                                // if-elseif without jmp
                                _processed.Process(newJpf);
                                control.AddIf(newJpfIndex, newJpf.Index);
                            }
                            else
                            {
                                // if-else without jmp
                                control.AddElse(jpf.Index, endOfBlock);
                            }

                            return;
                        }

                        // Isn't our jump
                        if (newJmp.Index != endOfBlock)
                        {
                            control.AddElse(jpf.Index, endOfBlock);
                            return;
                        }

                        jpf = newJpf;
                        jmp = newJmp;
                        _processed.Process(jpf);
                        _processed.TryProcess(jmp);

                        control.AddIf(newJpfIndex, jpf.Index);

                        if (jpf.Index == endOfBlock)
                            return;
                    }
                }

                private bool TryMakeGotoOrSkip()
                {
                    var instruction = _instructions[_index];
                    if (instruction is JMP @goto && _processed.TryProcess(@goto))
                    {
                        var control = new Goto(_instructions, _index, @goto.Index);
                        _result.Add(control);
                        return true;
                    }

                    if (!(instruction is JPF jpf))
                        return true;

                    if (!_processed.TryProcess(jpf))
                        return true;

                    _begin = jpf;
                    return false;
                }

                private bool TryMakeIf()
                {
                    var jpf = _begin;

                    var control = new If(_instructions, _index, _begin.Index);
                    _result.Add(control);

                    if (!(_instructions[_begin.Index - 1] is JMP jmp))
                    {
                        // There is no JMP instruction. Simple if {}
                        return true;
                    }

                    if (jmp.Index == jpf.Index)
                    {
                        // It isn't our jump, but an nested if. If { nested if{}<-}
                        return true;
                    }

                    if (jmp.Index < jpf.Index)
                    {
                        // It isn't our jump, but an nested loop. If { nested while{}<-}
                        return true;
                    }

                    if (jmp.Index < _index)
                    {
                        // It isn't our jump, but an nested goto. If { nested goto l;<-}
                        return true;
                    }

                    _processed.Process(jmp);
                    AddIfElseBranches(control, jpf, jmp);
                    return true;
                }

                private bool TryMakeWhile()
                {
                    if (_instructions[_begin.Index - 1] is JMP jmp && jmp.Index == _index)
                    {
                        _processed.TryProcess(jmp);
                        _result.Add(new While(_instructions, _index, _begin.Index));
                        return true;
                    }

                    return false;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}