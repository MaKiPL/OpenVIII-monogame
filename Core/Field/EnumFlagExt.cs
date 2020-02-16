namespace OpenVIII.Fields
{
    public static class EnumFlagExt
    {
        #region Methods

        public static Module._Toggles Flip(this Module._Toggles flagged, Module._Toggles flag)
                                                                    => flagged ^= flag;

        #endregion Methods
    }
}