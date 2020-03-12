namespace OpenVIII.Battle.Dat
{
    public struct Object
    {
        public ushort CVertices;
        public VertexData[] VertexData;
        public ushort CTriangles;
        public ushort CQuads;
        public ulong Padding;
        public Triangle[] Triangles;
        public Quad[] Quads;
    }
}