namespace OpenVIII.Battle
{
    public partial class Stage
    {
        /// <summary>
        /// This is helper struct that works along with VertexPosition to provide Clut, texture page
        /// and bool to decide if it's quad or triangle
        /// </summary>
        public struct GeometryInfoSupplier
        {
            public bool bQuad;
            public byte clut;
            public byte texPage;
        }
    }
}