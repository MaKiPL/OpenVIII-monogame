namespace OpenVIII.Fields
{
    public static class EnumFlagExt
    {
        #region Methods

        public static Toggles Flip(this Toggles flagged, Toggles flag) => flagged ^ flag;

        #endregion Methods
    }
}