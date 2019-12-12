using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII.Battle
{
    /// <summary>
    /// base is a rectangle. the top vertex is centered over the base.
    /// </summary>
    public class RegularPyramid
    {
        private VertexPositionColor[] uniqueVertices;
        //public VertexPositionColor[] Vertices { get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer Indices { get; private set; }
        Slide<float> slider;

        public RegularPyramid()
        {
            uniqueVertices = new VertexPositionColor[5];
            VertexBuffer = new VertexBuffer(Memory.graphics.GraphicsDevice, uniqueVertices[0].GetType(), 5, BufferUsage.WriteOnly);
            Indices = new IndexBuffer(Memory.graphics.GraphicsDevice, typeof(short), 18, BufferUsage.WriteOnly);
            slider = new Slide<float>(0f, MathHelper.TwoPi, TimeSpan.FromMilliseconds(1500d), MathHelper.Lerp)
            {
                Repeat = true
            };
            Set(1, 1, null);
        }

        public int Triangles { get; private set; }
        private void GenerateVertices()
        {
            short[] indices = new short[]
            {
                //side 1
                0,
                2,
                1,
                //side 2
                0,
                4,
                2,
                //side 3
                0,
                3,
                4,
                //side 4
                0,
                1,
                3,
                //base part 1
                1,
                2,
                3,
                //base part 2
                2,
                4,
                3,
            };
            Triangles = indices.Length / 3;
            tempVertices = (VertexPositionColor[])uniqueVertices.Clone();
            Indices.SetData(indices);
        }

        VertexPositionColor[] tempVertices;
        public void Update()
        {
            var r=slider.Update();
            var rotation = Matrix.CreateRotationY(r);
            for (int i = 0; i< tempVertices.Length; i++)
            {
                tempVertices[i].Position = Vector3.Transform(Vector3.Transform(uniqueVertices[i].Position, rotation),offset);

            }
            VertexBuffer.SetData(tempVertices);
        }
        public void Set(float height, float basewidth, params Color[] color) => Set(height, basewidth, basewidth, color);

        public void Set(float height, float basewidth, float baselength, params Color[] color)
        {
            if (color == null || color.Length == 0)
            {
                Color c = Color.Yellow;
                color = new Color[] { c, c, c, c, c };
            }
            else if (color.Length < uniqueVertices.Length)
            {
                Color c = color[0];
                color = new Color[] { c, c, c, c, c };
            }
            //techinally there are 5 unique. but everything is triangles.
            float bottom = height<0? -height: 0f;
            if (height < 0) height = 0;
            uniqueVertices[0].Position = new Vector3(0f, height, 0f);
            uniqueVertices[0].Color = color[0];
            uniqueVertices[1].Position = new Vector3(-basewidth / 2f, bottom, -baselength / 2f);
            uniqueVertices[1].Color = color[1];
            uniqueVertices[2].Position = new Vector3(-basewidth / 2f, bottom, baselength / 2f);
            uniqueVertices[2].Color = color[2];
            uniqueVertices[3].Position = new Vector3(basewidth / 2f, bottom, -baselength / 2f);
            uniqueVertices[3].Color = color[3];
            uniqueVertices[4].Position = new Vector3(basewidth / 2f, bottom, baselength / 2f);
            uniqueVertices[4].Color = color[4];
            GenerateVertices();
        }
        Matrix offset;
        public void Set(Matrix offset)
        {
            this.offset = offset;
        }
    }
}