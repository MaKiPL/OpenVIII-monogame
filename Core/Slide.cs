using System;

namespace OpenVIII
{
    public class Slide<T>
    {
        #region Fields

        private double _currentMS;
        private T _end;
        private Func<T, T, float, T> _function;
        private T _start;
        private double _totalMS;

        #endregion Fields

        #region Constructors

        public Slide(T start, T end, double totalMS, Func<T, T, float, T> function)
        {
            _start = start;
            _end = end;
            _totalMS = totalMS;
            _function = function;
        }

        #endregion Constructors

        #region Properties

        public double CurrentMS => _currentMS;
        public bool Done => _currentMS >= _totalMS;

        public T End { get => _end; set => _end = value; }
        public Func<T, T, float, T> Function { get => _function; set => _function = value; }
        public T Start { get => _start; set => _start = value; }
        public double TotalMS { get => _totalMS; set => _totalMS = value; }

        #endregion Properties

        #region Methods

        public void Restart() => _currentMS = 0d;

        public void Reverse()
        {
            T tmp = _start; _start = _end; _end = tmp;
        }

        public T Update()
        {
            if (!Done)
            {
                _currentMS += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                float percent = (float)(Done ? 1f : _currentMS / _totalMS);
                return _function(_start, _end, percent);
            }
            return _end;
        }

        #endregion Methods
    }
}