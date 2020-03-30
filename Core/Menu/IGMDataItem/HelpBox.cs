using System.Collections.Generic;

namespace OpenVIII.IGMDataItem
{
    public class HelpBox : IGMDataItem.Box
    {
        //public void AddTextChangeEvent(ref EventHandler<KeyValuePair<byte, FF8String>> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;

        //public void AddTextChangeEvent(ref EventHandler<FF8String> choiceChangeHandler) => choiceChangeHandler += TextChangeEvent;

        #region Methods

        public void TextChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => Data = e.Value;

        public void TextChangeEvent(object sender, KeyValuePair<ItemInMenu, FF8String> e) => Data = e.Value;

        public void TextChangeEvent(object sender, FF8String e) => Data = e;

        #endregion Methods
    }
}