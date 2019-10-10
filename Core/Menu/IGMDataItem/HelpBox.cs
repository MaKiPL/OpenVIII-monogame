using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII.IGMDataItem
{
    public class HelpBox : IGMDataItem.Box
    {
        #region Methods

        protected void TextChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => Data = e.Value;

        protected void TextChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => Data = e.Value;

        protected void TextChangeEvent(object sender, FF8String e) => Data = e;

        #endregion Methods

        #region Constructors

        public HelpBox(FF8String data = null, Rectangle? pos = null, Icons.ID? title = null, Box_Options options = Box_Options.Default) : base(data, pos, title, options)
        {
        }

        #endregion Constructors

        public void AddTextChangeEvent(ref EventHandler<KeyValuePair<Item_In_Menu, FF8String>> itemChangeHandler) => itemChangeHandler += TextChangeEvent;

        public void AddTextChangeEvent(ref EventHandler<KeyValuePair<byte, FF8String>> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;

        public void AddTextChangeEvent(ref EventHandler<FF8String> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;
    }
}