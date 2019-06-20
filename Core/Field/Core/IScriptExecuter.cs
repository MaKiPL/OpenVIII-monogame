using System.Collections.Generic;

namespace OpenVIII
{
    public interface IScriptExecuter
    {
        IEnumerable<IAwaitable> Execute(IServices services);
    }
}