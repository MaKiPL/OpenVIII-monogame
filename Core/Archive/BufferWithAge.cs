using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public class BufferWithAge : IReadOnlyList<byte>
    {
        #region Fields

        private readonly byte[] _buffer;

        #endregion Fields

        #region Constructors

        public BufferWithAge(byte[] buffer)
        {
            this._buffer = buffer;
            Used = Created = DateTime.Now;
        }

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyList<byte>)_buffer).Count;
        public DateTime Created { get; }
        public DateTime Used { get; set; }

        #endregion Properties

        #region Indexers

        public byte this[int index] => ((IReadOnlyList<byte>)_buffer)[index];

        #endregion Indexers

        #region Methods

        public static implicit operator BufferWithAge(byte[] @in) => new BufferWithAge(@in);

        public static implicit operator byte[] (BufferWithAge @in) => @in._buffer;

        public IEnumerator<byte> GetEnumerator() => ((IReadOnlyList<byte>)_buffer).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<byte>)_buffer).GetEnumerator();

        public DateTime Poke() => Used = DateTime.Now;

        public override string ToString() => $"{{{nameof(Created)}: {Created}, {nameof(Used)}: {Used}, Size: {_buffer?.Length ?? 0}}}";

        #endregion Methods
    }
}