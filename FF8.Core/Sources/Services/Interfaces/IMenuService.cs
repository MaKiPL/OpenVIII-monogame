using System;

namespace FF8.Core
{
    public interface IMenuService
    {
        Boolean IsSupported { get; }

        IAwaitable ShowEnterNameDialog(NamedEntity entity);
    }
}