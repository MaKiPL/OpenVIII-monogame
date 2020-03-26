using System.Collections.Generic;
using System.Linq;
using static OpenVIII.Fields.Scripts.Jsm;

namespace OpenVIII.Fields.Scripts.Instructions
{
    public interface IJsmInstruction
    {
    }

    public static class JsmInstructionEx
    {
        #region Methods

        public static IEnumerable<IJsmInstruction> Flatten(this IJsmInstruction e)
        {
            bool checktype(IJsmInstruction inst)
            {
                return
                    typeof(Control.While.WhileSegment) == inst.GetType() ||
                typeof(Control.If.ElseIfSegment) == inst.GetType() ||
                typeof(Control.If.ElseSegment) == inst.GetType() ||
                typeof(Control.If.IfSegment) == inst.GetType() ||
                typeof(Jsm.ExecutableSegment) == inst.GetType();
            }

            if (checktype(e))
            {
                var es = ((ExecutableSegment)e);
                foreach (var child in es.Where(x => checktype(x)).Select(x => (ExecutableSegment)x))
                {
                    foreach (var i in Flatten(child))
                        yield return i;
                }
                foreach (var child in es.Where(x => !checktype(x)))
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