using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Resume script, controlled animation. Returns this entity to its base animation.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/045_ANIMESTOP"/>
    public sealed class ANIMESTOP : JsmInstruction
    {
        public ANIMESTOP()
        {
        }

        public ANIMESTOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(ANIMESTOP)}()";
        }
    }
}