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
        private const float persistentAlpha = .9f;

        #region Fields

        private static readonly TimeSpan RotationTime = TimeSpan.FromMilliseconds(1500d);
        private static readonly TimeSpan FadeTime = TimeSpan.FromMilliseconds(1000);
        private BasicEffect effect;
        private Matrix offset;
        private Slide<float> radians;
        private Slide<float> fader;
        private VertexPositionColor[] tempVertices;
        private VertexPositionColor[] uniqueVertices;

        #endregion Fields

        #region Constructors

        public RegularPyramid()
        {
            uniqueVertices = new VertexPositionColor[5];
            VertexBuffer = new VertexBuffer(Memory.graphics.GraphicsDevice, uniqueVertices[0].GetType(), 5, BufferUsage.WriteOnly);
            Indices = new IndexBuffer(Memory.graphics.GraphicsDevice, typeof(short), 18, BufferUsage.WriteOnly);
            radians = new Slide<float>(0f, MathHelper.TwoPi, RotationTime, MathHelper.Lerp)
            {
                Repeat = true
            };

            fader = new Slide<float>(0f, 1f, FadeTime, MathHelper.Lerp);
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            Set(1, 1, null);
        }

        #endregion Constructors

        #region Properties

        private IndexBuffer Indices { get; set; }
        private int Triangles { get; set; }
        private VertexBuffer VertexBuffer { get; set; }

        #endregion Properties

        #region Methods

        public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            //donno why but direct x crashes when i try to draw colored primatives.
            if (Memory.currentGraphicMode == Memory.GraphicModes.DirectX || alpha < float.Epsilon) return;
            effect.World = worldMatrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;
            effect.VertexColorEnabled = true;
            effect.Alpha = alpha * persistentAlpha;

            //PyramidEffect.EnableDefaultLighting();

            Memory.graphics.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Memory.graphics.GraphicsDevice.Indices = Indices;
            RasterizerState tmp = Memory.graphics.GraphicsDevice.RasterizerState;
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount);
            }
            Memory.graphics.GraphicsDevice.RasterizerState = tmp;
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
            float bottom = height < 0 ? -height : 0f;
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
            FadeIn();
        }

        public void Set(Vector3 offset) => this.offset = Matrix.CreateTranslation(offset);

        private float alpha;

        public void FadeIn()
        {
            if (fader.Reversed)
            {
                fader.ReverseRestart();
            }
            else fader.Restart();
        }

        public void FadeOut()
        {
            if (!fader.Reversed)
            {
                fader.ReverseRestart();
            }
            else fader.Restart();
        }

        public void Update()
        {
            // Update Fade
            alpha = fader.Update();
            // Update Rotation
            Matrix rotation = Matrix.CreateRotationY(radians.Update());
            for (int i = 0; i < tempVertices.Length; i++)
            {
                tempVertices[i].Position = Vector3.Transform(Vector3.Transform(uniqueVertices[i].Position, rotation), offset);
            }
            VertexBuffer.SetData(tempVertices);
        }

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

        public void Hide()
        {
            FadeOut();
            alpha = 0f;
            fader.GotoEnd();
        }

        #endregion Methods
    }
}