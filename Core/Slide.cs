using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    public class Slide<T>
    {
        T _start;
        T _end;
        double _time;
        double _currenttime;
        Func<T, T, float, T> _function;

        public Slide(T start, T end, double totaltime, Func<T, T, float, T> function)
        {
            _start = start;
            _end = end;
            _time = totaltime;
            _function = function;
        }

        public bool Done => _currenttime >= _time;
        public T Update()
        {
            if (!Done)
            {
                _currenttime += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                float percent = (float)(Done?1f:_currenttime / _time);
                return _function(_start, _end, percent);
            }
            return _end;
        }
        public void Restart() => _currenttime = 0d;
        public void Reverse() { T tmp = _start; _start = _end; _end = tmp; }
    }
}
