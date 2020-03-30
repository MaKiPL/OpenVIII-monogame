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

            private sealed class ProcessedJumps
            {
                #region Fields

                private readonly HashSet<IJumpToInstruction> _processed = new HashSet<IJumpToInstruction>();

                #endregion Fields

                #region Constructors

                public ProcessedJumps()
                {
                }

                #endregion Constructors

                #region Methods

                public void Process(IJumpToInstruction jmp)
                {
                    if (!_processed.Add(jmp))
                        throw new InvalidProgramException($"The jump instruction ({jmp}) has already been processed.");
                }

                public bool TryProcess(IJumpToInstruction jmp) => _processed.Add(jmp);

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}