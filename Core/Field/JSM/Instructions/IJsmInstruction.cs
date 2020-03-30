using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static OpenVIII.Fields.Scripts.Jsm;

#pragma warning disable 184

namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJsmInstruction
    {
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class JsmInstructionEx
    {
        #region Methods

        public static IEnumerable<IJsmInstruction> Flatten(this IJsmInstruction e)
        {
            bool checkType(IJsmInstruction inst)
            {
                return
                    (inst is Control.While.WhileSegment ||
                     inst is Control.If.ElseIfSegment ||
                     inst is Control.If.ElseSegment ||
                     inst is Control.If.IfSegment ||
                     inst is ExecutableSegment);
            }

            if (checkType(e))
            {
                var es = ((ExecutableSegment)e);
                foreach (var child in es.Where(checkType).Select(x => (ExecutableSegment)x))
                {
                    foreach (var i in Flatten(child))
                        yield return i;
                }
                foreach (var child in es.Where(x => !checkType(x)))
                {
                    yield return child;
                }
            }
            else
                yield return e;
        }

        #endregion Methods
    }
}