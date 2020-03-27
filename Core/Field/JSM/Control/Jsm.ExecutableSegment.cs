using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public class ExecutableSegment : Segment, IJsmInstruction, IEnumerable<IJsmInstruction>
        {
            #region Constructors

            public ExecutableSegment(int from, int to)
                : base(from, to)
            {
            }

            #endregion Constructors

            #region Methods

            public IEnumerator<IJsmInstruction> GetEnumerator() => _list.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

            public virtual IScriptExecutor GetExecuter() => GetExecuter(_list);

            internal static IScriptExecutor GetExecuter(IEnumerable<IJsmInstruction> instructions) => new Executor(instructions);

            #endregion Methods

            #region Classes

            private sealed class Executor : IScriptExecutor, IEnumerable<IJsmInstruction>
            {
                #region Fields

                private readonly IEnumerable<IJsmInstruction> _list;

                #endregion Fields

                #region Constructors

                public Executor(IEnumerable<IJsmInstruction> list) => _list = list;

                #endregion Constructors

                #region Methods

                public IEnumerable<IAwaitable> Execute(IServices services)
                {
                    foreach (var instr in _list)
                    {
                        if (instr is JsmInstruction singleInstruction)
                        {
                            yield return singleInstruction.Execute(services);
                        }
                        else if (instr is ExecutableSegment segment)
                        {
                            // TODO: Change recursion to the loop
                            var nested = segment.GetExecuter();
                            foreach (var result in nested.Execute(services))
                                yield return result;
                        }
                        else
                        {
                            throw new NotSupportedException($"Cannot execute instruction [{instr}] of type [{instr.GetType()}].");
                        }
                    }
                }

                public IEnumerator<IJsmInstruction> GetEnumerator() => _list.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}