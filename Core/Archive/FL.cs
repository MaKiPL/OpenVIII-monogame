using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class FL : IReadOnlyList<string>
    {
        #region Fields

        private List<string> data;

        #endregion Fields

        #region Constructors

        public FL(byte[] buffer) => data = System.Text.Encoding.UTF8.GetString(buffer).Split('\n').Select(x => x.TrimEnd()).ToList();

        public FL(StreamWithRangeValues fL)
        {
            using (StreamReader br = new StreamReader(fL, System.Text.Encoding.UTF8))
            {
                fL.Seek(fL.Offset, SeekOrigin.Begin);
                while (fL.Position < fL.Max)
                {
                    data.Add(br.ReadLine().TrimEnd());
                }
            }
        }

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