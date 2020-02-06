using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public class FL : IReadOnlyList<string>
    {
        #region Fields

        private List<string> data;

        #endregion Fields

        #region Constructors

        public FL(byte[] buffer) => data = new List<string>(System.Text.Encoding.UTF8.GetString(buffer).Split('\n'));

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyList<string>)data).Count;

        #endregion Properties

        #region Indexers

        public string this[int index] => ((IReadOnlyList<string>)data)[index].TrimEnd();

        #endregion Indexers

        #region Methods

        public IEnumerator<string> GetEnumerator() => ((IReadOnlyList<string>)data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<string>)data).GetEnumerator();

        #endregion Methods
    }
}