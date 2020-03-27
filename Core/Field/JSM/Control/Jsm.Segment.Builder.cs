using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public partial class Segment
        {
            #region Classes

            public static class Builder
            {
                #region Methods

                public static ExecutableSegment Build(List<JsmInstruction> instructions, IReadOnlyList<IJsmControl> controls)
                {
                    var dic = new Dictionary<int, List<Segment>>();
                    foreach (var control in controls)
                    {
                        foreach (var seg in control.EnumerateSegments())
                        {
                            if (!dic.TryGetValue(seg.From, out var list))
                            {
                                list = new List<Segment>();
                                dic.Add(seg.From, list);
                            }

                            list.Add(seg);
                        }
                    }

                    var rootSegment = new ExecutableSegment(0, instructions.Count);
                    dic.Add(0, new List<Segment> { rootSegment });

                    var segments = new Stack<Segment>();
                    Segment segment = rootSegment;

                    var instructionsCount = instructions.Count;
                    for (var i = 0; i < instructionsCount; i++)
                    {
                        while (segment.To <= i)
                            segment = segments.Pop();

                        if (i > 0 && dic.TryGetValue(i, out var nestedSegments))
                        {
                            foreach (var seg in nestedSegments)
                            {
                                segments.Push(segment);
                                if (seg is ExecutableSegment executable)
                                    segment.Add(executable);

                                segment = seg;

                                if (!(instructions[i] is IJumpToInstruction))
                                    segment.Add(instructions[i]);
                            }
                        }
                        else
                        {
                            segment.Add(instructions[i]);
                        }
                    }

                    if (segments.Count != 0)
                        throw new InvalidProgramException("Failed to join code segments.");

                    return rootSegment;
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}