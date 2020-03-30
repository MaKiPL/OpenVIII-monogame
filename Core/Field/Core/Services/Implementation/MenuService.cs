using System;

namespace OpenVIII.Fields
{
    public sealed class MenuService : IMenuService
    {
        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public IAwaitable ShowEnterNameDialog(NamedEntity entity)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MenuService)}.{nameof(ShowEnterNameDialog)}({nameof(entity)}: {entity})");
            return DummyAwaitable.Instance;
        }

        #endregion Methods
    }
}