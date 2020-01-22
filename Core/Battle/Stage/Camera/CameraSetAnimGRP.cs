namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Structs

        public struct CameraSetAnimGRP
        {
            #region Constructors

            public CameraSetAnimGRP(int set, int anim)
            {
                Set = set;
                Anim = anim;
            }

            #endregion Constructors

            #region Properties

            public int Anim { get; private set; }
            public int Set { get; private set; }

            #endregion Properties
        }

        #endregion Structs
    }
}