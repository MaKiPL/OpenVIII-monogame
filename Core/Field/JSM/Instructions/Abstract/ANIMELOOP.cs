using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Fields.Scripts.Instructions.Abstract
{
    public abstract class ANIMELOOP: ANIME
    {
        /// <summary>
        /// First frame of animation
        /// </summary>
        protected readonly Int32 _firstFrame;
        /// <summary>
        /// Last frame of animation
        /// </summary>
        protected readonly Int32 _lastFrame;


        public ANIMELOOP(Int32 animationId, Int32 firstFrame, Int32 lastFrame) : base(animationId)
        {
            _firstFrame = firstFrame;
            _lastFrame = lastFrame;
        }

        public ANIMELOOP(Int32 animationId, IStack<IJsmExpression> stack)
            : this(animationId,
                lastFrame: ((IConstExpression)stack.Pop()).Int32(),
                firstFrame: ((IConstExpression)stack.Pop()).Int32())
        {
        }
    }
}
