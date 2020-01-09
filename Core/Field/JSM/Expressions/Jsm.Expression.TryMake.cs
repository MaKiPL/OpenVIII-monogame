using System;

namespace OpenVIII.Fields
{
    public static partial class Jsm
    {
        public static partial class Expression
        {
            public static IJsmExpression TryMake(Jsm.Opcode opcode, Int32 param, IStack<IJsmExpression> stack)
            {
                IJsmExpression result = TryMakeInternal(opcode, param, stack);
                return result?.Evaluate(StatelessServices.Instance); // Simplify the expression
            }

            private static IJsmExpression TryMakeInternal(Jsm.Opcode opcode, Int32 param, IStack<IJsmExpression> stack)
            {
                switch (opcode)
                {
                    case Jsm.Opcode.PSHN_L:
                        return new PSHN_L(param);
                    case Jsm.Opcode.PSHAC:
                        return new PSHAC(new Jsm.FieldObjectId(param));
                    case Jsm.Opcode.PSHI_L:
                        return new PSHI_L(new ScriptResultId(param));
                    case Jsm.Opcode.PSHM_B:
                        return new PSHM_B(new GlobalVariableId<Byte>(param));
                    case Jsm.Opcode.PSHM_W:
                        return new PSHM_W(new GlobalVariableId<UInt16>(param));
                    case Jsm.Opcode.PSHM_L:
                        return new PSHM_L(new GlobalVariableId<UInt32>(param));
                    case Jsm.Opcode.PSHSM_B:
                        return new PSHSM_B(new GlobalVariableId<SByte>(param));
                    case Jsm.Opcode.PSHSM_W:
                        return new PSHSM_W(new GlobalVariableId<Int16>(param));
                    case Jsm.Opcode.PSHSM_L:
                        return new PSHSM_L(new GlobalVariableId<Int32>(param));
                    case Jsm.Opcode.CAL:
                        return CAL.Read(param, stack);
                    //case Opcode.MOVIEREADY:
                    //    return new Instruction.MOVIEREADY(stack.Pop(), stack.Pop());
                }

                return null;
            }
        }
    }
}