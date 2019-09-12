using System;
using System.Diagnostics;

namespace OpenVIII
{
    public class Slide<T>
    {
        #region Fields

        private T _current;
        private double _currentMS;
        private float _currentPercent;
        private T _end;
        private Func<T, T, float, T> _function;
        private T _start;
        private double _totalMS;
        private double _reverseMS = 0;
        private double _delayMS = 0;

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

        public T Current => _current;//Done ? _end : _current;
        public double CurrentMS => _currentMS;
        public float CurrentPercent => _currentPercent;
        public bool Done => _currentMS >= _totalMS;

        public T End { get => _end; set => _end = value; }
        public Func<T, T, float, T> Function { get => _function; set => _function = value; }
        public T Start { get => _start; set => _start = value; }
        public double TotalMS { get => _totalMS; set => _totalMS = value; }
        public double ReverseMS { get => _reverseMS; set => _reverseMS = value; }
        public bool Reversed { get; private set; } = false;
        /// <summary>
        /// When started wait this many MS before moving.
        /// </summary>
        public double DelayMS { get => _delayMS; set => _delayMS = value; }

        #endregion Properties

        #region Methods

        public void Restart() => _currentMS = 0d;
        public void ReverseRestart()
        {
            Reverse();
            Restart();
        }
        public void Reverse()
        {
            T tmp = _start;
            _start = _end;
            _end = tmp;
            if (_reverseMS > double.Epsilon)
            {
                double t = _reverseMS;
                _reverseMS = _totalMS;
                _totalMS = t;
            }
            Reversed = !Reversed;
        }

        public T Update()
        {
            if (!Done && _function != null)
            {
                UpdatePercent();
                _current = _function(_start, _end, _currentPercent);
                return _current;
            }
            return _end;
        }

        public float UpdatePercent()
        {
            //Debug.WriteLine($"{this}::{Memory.gameTime.ElapsedGameTime.TotalMilliseconds}");
            _currentPercent = 1f;
            if (!Done)
            {
                _currentMS += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                return _currentPercent = _currentMS < _delayMS? 0f: (float)(Done ? 1f : (_currentMS- _delayMS) / _totalMS);
            }
            else
                return _currentPercent;
        }

        #endregion Methods
    }
}