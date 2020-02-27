using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set HP
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/146_SETHP&action=edit&redlink=1"/>
    public sealed class SETHP : JsmInstruction
    {
        private readonly Characters _character;
        private readonly Int32 _hp;

        public SETHP(Characters character, Int32 hp)
        {
            _character = character;
            _hp = hp;
        }

        public SETHP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                hp: ((IConstExpression)stack.Pop()).Int32(),
                character: ((IConstExpression)stack.Pop()).Characters())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETHP)}({nameof(_character)}: {_character}, {nameof(_hp)}: {_hp})";
        }
    }
}