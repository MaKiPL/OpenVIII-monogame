using System;

namespace OpenVIII
{
    public interface IMenuService
    {
        Boolean IsSupported { get; }

        IAwaitable ShowEnterNameDialog(NamedEntity entity);
    }
}