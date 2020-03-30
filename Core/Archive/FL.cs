using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    // ReSharper disable once InconsistentNaming
    public class FL : IReadOnlyList<string>
    {
        #region Fields

        private readonly List<string> _data;

        #endregion Fields

        #region Constructors

        public FL(byte[] buffer) => _data = System.Text.Encoding.UTF8.GetString(buffer).Split('\n').Select(x => x.TrimEnd()).ToList();

        public FL(StreamWithRangeValues fL)
        {
            using (var br = new StreamReader(fL, System.Text.Encoding.UTF8))
            {
                fL.Seek(fL.Offset, SeekOrigin.Begin);
                while (fL.Position < fL.Max)
                {
                    _data.Add(br.ReadLine()?.TrimEnd());
                }
            }
        }

        #endregion Constructors

        #region Properties

        public int Count => _data.Count;

        #endregion Properties

        #region Indexers

        public string this[int index] => _data[index].TrimEnd();

        #endregion Indexers

        #region Methods

        public IEnumerator<string> GetEnumerator() => ((IReadOnlyList<string>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<string>)_data).GetEnumerator();

        #endregion Methods
    }
}