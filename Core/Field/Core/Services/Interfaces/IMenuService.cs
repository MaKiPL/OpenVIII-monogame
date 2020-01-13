using System;

namespace OpenVIII.Fields
{
    public interface IMenuService
    {
        Boolean IsSupported { get; }

        IAwaitable ShowEnterNameDialog(NamedEntity entity);
    }
}