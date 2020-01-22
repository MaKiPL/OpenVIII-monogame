namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Structs

        /// <summary>
        /// Battle camera settings are about 32 bytes of unknown flags and variables used in whole
        /// stage including geometry
        /// </summary>
        public struct BattleCameraSettings
        {
            #region Fields

            public byte[] unk;

            #endregion Fields
        }

        #endregion Structs
    }
}