using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    public class Triggers : IEnumerable<Trigger>, IReadOnlyList<Trigger>
    {
        #region Fields

        private List<Trigger> triggers;

        #endregion Fields

        #region Constructors

        public Triggers() => this.triggers = new List<Trigger>(12);

        #endregion Constructors

        #region Properties

        public int Count => ((IReadOnlyList<Trigger>)triggers).Count;

        #endregion Properties

        #region Indexers

        public Trigger this[int index] => ((IReadOnlyList<Trigger>)triggers)[index];

        #endregion Indexers

        #region Methods

        public static Triggers Read(BinaryReader br)
        {
            var t = new Triggers();
            foreach (var i in Enumerable.Range(0, t.triggers.Capacity))
                t.triggers.Add(Trigger.Read(br));
            return t;
        }

        public IEnumerator<Trigger> GetEnumerator() => ((IEnumerable<Trigger>)triggers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Trigger>)triggers).GetEnumerator();

        #endregion Methods
    }
}