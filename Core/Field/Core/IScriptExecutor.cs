using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public interface IScriptExecutor
    {
        #region Methods

        IEnumerable<IAwaitable> Execute(IServices services);

        #endregion Methods
    }
}