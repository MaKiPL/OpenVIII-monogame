namespace OpenVIII.Battle.Dat
{
    public struct Object
    {
        #region Fields

        public ushort CQuads;
        public ushort CTriangles;
        public ushort CVertices;
        public ulong Padding;
        public Quad[] Quads;
        public Triangle[] Triangles;
        public VertexData[] VertexData;

        #endregion Fields
    }
}