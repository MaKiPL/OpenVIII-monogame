using OpenVIII.Encoding.Tags;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Inputs
    {
        #region Properties

        public abstract Dictionary<List<FF8TextTagKey>, List<InputButton>> Data { get; protected set; }

        public abstract bool DrawGamePadButtons { get; }

        #endregion Properties
    }
}