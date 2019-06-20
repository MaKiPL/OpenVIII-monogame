using System.Collections.Generic;

namespace FF8
{
    public interface IScriptExecuter
    {
        IEnumerable<IAwaitable> Execute(IServices services);
    }
}