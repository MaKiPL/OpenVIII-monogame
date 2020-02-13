using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public class BufferWithAge : IReadOnlyList<byte>
    {
        #region Fields

        private byte[] buffer;

        #endregion Fields

        #region Constructors

        public BufferWithAge(byte[] buffer)
        {
            this.buffer = buffer;
            Touched = Created = getTimeSpan();
        }

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyList<byte>)buffer).Count;
        public TimeSpan Created { get; }
        public TimeSpan Touched { get; private set; }

        #endregion Properties

        #region Indexers

        public byte this[int index] => ((IReadOnlyList<byte>)buffer)[index];

        #endregion Indexers

        #region Methods

        public static implicit operator BufferWithAge(byte[] @in) => new BufferWithAge(@in);

        public static implicit operator byte[] (BufferWithAge @in) => @in.buffer;

        public IEnumerator<byte> GetEnumerator() => ((IReadOnlyList<byte>)buffer).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<byte>)buffer).GetEnumerator();

        public TimeSpan Poke() => Touched = getTimeSpan();

        private static TimeSpan getTimeSpan() => Memory.ElapsedGameTime;

        #endregion Methods
    }
}