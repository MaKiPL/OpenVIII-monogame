using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMData.Group
{
    public class TopMenu : IGMData.Group.Base
    {
        #region Methods

        public static new TopMenu Create(params Menu_Base[] d)
        {
            if (d[0].GetType() == typeof(IGMDataItem.Box))
            {
                return Create<TopMenu>(d);
            }
            throw new ArgumentException($"First argument must be {typeof(IGMDataItem.Box)}");
        }

        public override void Refresh()
        {
            if (Enabled)
            {
                Cursor_Status |= (Cursor_Status.Draw | Cursor_Status.Blinking);
                if (ITEM[0, 0].GetType() == typeof(IGMDataItem.Box))
                {
                    ((IGMDataItem.Box)ITEM[0, 0]).Draw(true);
                    Menu.BoxReturn dims = ((IGMDataItem.Box)ITEM[0, 0]).Dims;
                    if (dims.Cursor != Point.Zero)
                        CURSOR[0] = dims.Cursor;
                }
            }
            base.Refresh();
        }

        protected override void Init()
        {
            base.Init();
            Hide();
        }

        #endregion Methods
    }
}