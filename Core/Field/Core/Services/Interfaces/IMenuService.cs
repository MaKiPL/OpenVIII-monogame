using System;

namespace OpenVIII.Fields
{
    public interface IMenuService
    {
        #region Properties

        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        IAwaitable ShowEnterNameDialog(NamedEntity entity);

        #endregion Methods
    }
}