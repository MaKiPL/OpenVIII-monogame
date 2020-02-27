using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Loops the given frames of an animation. Resume script, Play controlled looping animation
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/036_RCANIMELOOP"/>
    public sealed class RCANIMELOOP : Abstract.ANIMELOOP
    {
        public RCANIMELOOP(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        public RCANIMELOOP(int animationId, int firstFrame, int lastFrame) : base(animationId, firstFrame, lastFrame)
        {
        }

        public override String ToString()
        {
            return $"{nameof(RCANIMELOOP)}({nameof(_animationId)}: {_animationId}, {nameof(_firstFrame)}: {_firstFrame}, {nameof(_lastFrame)}: {_lastFrame})";
        }
    }
}