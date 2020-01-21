namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #endregion Methods
        #region Structs

        /// <summary>
        /// Main struct for collection of camera animations. Every BattleCameraSet hold 8 animations
        /// no matter what
        /// </summary>
        private struct BattleCameraCollection
        {
            #region Fields

            public BattleCameraSet[] battleCameraSet;
            public uint cAnimCollectionCount;
            public uint pCameraEOF;

            #endregion Fields
        }

        #endregion Structs
    }
}