using System;

namespace FF8
{
    public interface IMenuService
    {
        Boolean IsSupported { get; }

        IAwaitable ShowEnterNameDialog(NamedEntity entity);
    }
}