using System.Collections.Concurrent;

namespace OpenVIII.Battle
{
    /// <summary>
    /// Animation system. Decided to go for struct, so I can attach it to instance and manipulate
    /// easily grouped. It's also open for modifications
    /// </summary>
    public struct AnimationSystem
    {
        #region Fields

        public ConcurrentQueue<int> AnimationQueue;

        private int _animationFrame;

        private int _animationId;

        private int _lastAnimationFrame;

        private int _lastAnimationId;

        private bool bAnimationStopped;

        #endregion Fields

        #region Properties

        public int AnimationFrame
        {
            get => _animationFrame; set
            {
                _lastAnimationFrame = _animationFrame;
                _animationFrame = value;
                if (_animationFrame > 0 && _lastAnimationId != _animationId)
                    _lastAnimationId = _animationId;
            }
        }

        public int AnimationId
        {
            get => _animationId; set
            {
                _lastAnimationId = _animationId;
                _animationId = value;
                AnimationFrame = 0;
            }
        }

        public bool AnimationStopped => bAnimationStopped;

        public int LastAnimationFrame { get => _lastAnimationFrame; private set => _lastAnimationFrame = value; }

        public int LastAnimationId { get => _lastAnimationId; private set => _lastAnimationId = value; }

        #endregion Properties

        #region Methods

        public int NextFrame() => ++AnimationFrame;

        public bool StartAnimation() => bAnimationStopped = false;

        public bool StopAnimation()
        {
            LastAnimationFrame = AnimationFrame;
            AnimationId = AnimationId;
            return bAnimationStopped = true;
        }

        #endregion Methods
    }
}