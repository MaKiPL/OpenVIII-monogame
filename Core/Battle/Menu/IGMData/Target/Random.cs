using System.Collections;
using System.Linq;

namespace OpenVIII.IGMData.Target
{
    public class Random
    {

        #region Fields

        private BitArray fields = new BitArray(2, false);

        #endregion Fields

        #region Constructors

        public Random()
        {

        }

        /// <summary>
        /// Set all feilds to a true/false value
        /// </summary>
        /// <param name="value">true/false</param>
        public Random(bool value)
        {
            fields.SetAll(value);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// if true the side (enemy or party) is randomized
        /// </summary>
        public bool Side { get => fields[0]; set => fields[0] = value; }

        /// <summary>
        /// if true only a single target is chosen at random
        /// </summary>
        public bool Single { get => fields[1]; set => fields[1] = value; }

        #endregion Properties

        #region Methods

        public static implicit operator Random(bool value) => new Random(value);

        #endregion Methods

    }
}