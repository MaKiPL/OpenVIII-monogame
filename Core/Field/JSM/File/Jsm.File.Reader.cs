using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public static List<GameObject> Read(Byte[] data)
            {
                unsafe
                {
                    fixed (Byte* ptr = data)
                    {
                        if (ptr == null) return null;
                        var header = (Header*)ptr;
                        var areas = (Group*)(ptr + sizeof(Header));
                        var doors = areas + header->CountAreas;
                        var modules = doors + header->CountDoors;
                        var objects = modules + header->CountModules;
                        var end = objects + header->CountObjects;
                        var scripts = (Script*)(ptr + header->ScriptsOffset);
                        var operation = (Operation*)(ptr + header->OperationsOffset);

                        var groupNumber = end - areas;
                        var groups = new Group[groupNumber];
                        for (var group = areas; group < end; group++)
                            groups[--groupNumber] = *group;

                        var gameObjects = new List<GameObject>(groups.Length);

                        foreach (var group in groups.OrderBy(g => g.Label))
                        {
                            var objectScripts = new List<GameScript>(group.ScriptsCount + 1);

                            for (var s = 0; s <= group.ScriptsCount; s++)
                            {
                                var scriptLabel = group.Label + s;

                                var position = scripts->Position;
                                scripts++;

                                var count = (UInt16)(scripts->Position - position);
                                var scriptSegment = MakeScript(operation + position, count);

                                objectScripts.Add(new GameScript(scriptLabel, scriptSegment));
                            }

                            gameObjects.Add(new GameObject(group.Label, objectScripts));
                        }

                        return gameObjects;
                    }
                }
            }

            private static unsafe Jsm.ExecutableSegment MakeScript(Operation* operation, UInt16 count)
            {
                var instructions = new List<JsmInstruction>(count / 2);
                var stack = new LabeledStack();
                var labelBuilder = new LabelBuilder(count);

                for (var i = 0; i < count; i++)
                {
                    var opcode = operation->Opcode;
                    var parameter = operation->Parameter;
                    operation++;

                    stack.CurrentLabel = i;
                    var expression = Jsm.Expression.TryMake(opcode, parameter, stack);
                    if (expression != null)
                    {
                        stack.Push(expression);
                        continue;
                    }

                    var instruction = JsmInstruction.TryMake(opcode, parameter, stack);
                    if (instruction != null)
                    {
                        labelBuilder.TraceInstruction(i, stack.CurrentLabel, new IndexedInstruction(instructions.Count, instruction));
                        instructions.Add(instruction);
                        continue;
                    }

                    throw new NotSupportedException(opcode.ToString());
                }

                if (stack.Count != 0)
                    throw new InvalidProgramException("Stack unbalanced.");

                if (!(instructions.First() is LBL))
                    throw new InvalidProgramException("Script must start with a label.");

                if (!(instructions.Last() is IRET))
                    throw new InvalidProgramException("Script must end with a return.");

                // Switch from opcodes to instructions
                var labelIndices = labelBuilder.Commit();

                // Merge similar instructions
                instructions = InstructionMerger.Merge(instructions, labelIndices);

                // Combine instructions to logical blocks
                var controls = Jsm.Control.Builder.Build(instructions);

                // Arrange instructions by segments and return root
                return Jsm.Segment.Builder.Build(instructions, controls);
            }
        }
    }
}