using System;

namespace FF8
{
    public sealed class MenuService : IMenuService
    {
        public Boolean IsSupported => true;

        public IAwaitable ShowEnterNameDialog(NamedEntity entity)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(MenuService)}.{nameof(ShowEnterNameDialog)}({nameof(entity)}: {entity})");
            return DummyAwaitable.Instance;
        }
    }
}