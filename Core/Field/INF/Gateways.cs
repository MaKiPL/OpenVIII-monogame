using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    public class Gateways : IEnumerable<Gateway>, IReadOnlyList<Gateway>
    {
        #region Fields

        private List<Gateway> gateways;

        #endregion Fields

        #region Constructors

        public Gateways() => this.gateways = new List<Gateway>(12);

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyList<Gateway>)gateways).Count;

        #endregion Properties

        #region Indexers

        public Gateway this[int index] => ((IReadOnlyList<Gateway>)gateways)[index];

        #endregion Indexers

        #region Methods

        public static Gateways Read(BinaryReader br, int type)
        {
            Gateways g = new Gateways();
            foreach (int i in Enumerable.Range(0, g.gateways.Capacity))
                g.gateways.Add(Gateway.Read(br, type));
            return g;
        }

        public IEnumerator<Gateway> GetEnumerator() => ((IEnumerable<Gateway>)gateways).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Gateway>)gateways).GetEnumerator();

        #endregion Methods
    }
}