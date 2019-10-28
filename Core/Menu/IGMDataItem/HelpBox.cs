using System;
using System.Collections.Generic;

namespace OpenVIII.IGMDataItem
{
    public class HelpBox : IGMDataItem.Box
    {
        #region Methods

        public void AddTextChangeEvent(ref EventHandler<KeyValuePair<Item_In_Menu, FF8String>> itemChangeHandler) => itemChangeHandler += TextChangeEvent;

        public void AddTextChangeEvent(ref EventHandler<KeyValuePair<byte, FF8String>> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;

        public void AddTextChangeEvent(ref EventHandler<FF8String> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;

        protected void TextChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => Data = e.Value;

        protected void TextChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => Data = e.Value;

        protected void TextChangeEvent(object sender, FF8String e) => Data = e;

        #endregion Methods
    }
}