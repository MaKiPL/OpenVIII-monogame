namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #endregion Methods
        #region Structs

        /// <summary>
        /// Struct for battle camera animation set. Animation set always contain 8 animations. This
        /// struct does not contain a data for pre-readed information. Therefore you have to call
        /// ReadAnimation(index) to actually read the animation to cam(CameraStruct).
        /// That is because there are extreme amount of cases where the camera is changing and
        /// reading again and again not including the battle stage. Also reading all camera
        /// animations is waste of time and resources
        /// </summary>
        private struct BattleCameraSet
        {
            #region Fields

            public uint[] animPointers;
            public uint globalSetPointer;

            #endregion Fields
        }

        #endregion Structs
    }
}