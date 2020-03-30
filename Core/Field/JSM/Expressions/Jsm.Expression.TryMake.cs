namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Methods

            public static IJsmExpression TryMake(Jsm.Opcode opcode, int param, IStack<IJsmExpression> stack)
            {
                var result = TryMakeInternal(opcode, param, stack);
                return result?.Evaluate(StatelessServices.Instance); // Simplify the expression
            }

            private static IJsmExpression TryMakeInternal(Jsm.Opcode opcode, int param, IStack<IJsmExpression> stack)
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
                        return new PSHM_B(new GlobalVariableId<byte>(param));

                    case Jsm.Opcode.PSHM_W:
                        return new PSHM_W(new GlobalVariableId<ushort>(param));

                    case Jsm.Opcode.PSHM_L:
                        return new PSHM_L(new GlobalVariableId<uint>(param));

                    case Jsm.Opcode.PSHSM_B:
                        return new PSHSM_B(new GlobalVariableId<sbyte>(param));

                    case Jsm.Opcode.PSHSM_W:
                        return new PSHSM_W(new GlobalVariableId<short>(param));

                    case Jsm.Opcode.PSHSM_L:
                        return new PSHSM_L(new GlobalVariableId<int>(param));

                    case Jsm.Opcode.CAL:
                        return CAL.Read(param, stack);
                        //case Opcode.MOVIEREADY:
                        //    return new Instruction.MOVIEREADY(stack.Pop(), stack.Pop());
                }

                return null;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}