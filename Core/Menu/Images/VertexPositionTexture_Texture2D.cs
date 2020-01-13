using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public class VertexPositionTexture_Texture2D : IDisposable
    {
        #region Fields

        public TextureHandler Texture;
        public VertexPositionTexture[] VPT;
        private AlphaTestEffect ate;

        /// <summary>
        /// Buffer that holds the VPT after transforming the location.
        /// </summary>
        private VertexPositionTexture[] TransformedVPT;

        #endregion Fields

        #region Constructors

        public VertexPositionTexture_Texture2D(VertexPositionTexture[] vPT, TextureHandler texture)
        {
            VPT = vPT;
            TransformedVPT = (VertexPositionTexture[])VPT.Clone();
            Texture = texture;

            ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Texture = (Texture2D)Texture
            };
        }

        #endregion Constructors

        #region Destructors

        ~VertexPositionTexture_Texture2D()
        {
            Dispose();
        }

        #endregion Destructors

        #region Methods

        public static VertexPositionTexture_Texture2D operator +(VertexPositionTexture_Texture2D left, VertexPositionTexture_Texture2D right)
        {
            if (left.Texture == right.Texture)
            {
                //could be optimized.
                List<VertexPositionTexture> tmp = new List<VertexPositionTexture>(left.VPT.Length + right.VPT.Length);
                tmp.AddRange(left.VPT);
                tmp.AddRange(right.VPT);
                return new VertexPositionTexture_Texture2D(tmp.ToArray(), left.Texture);
            }
            else throw new Exception("Textures must match or else won't work");
        }

        public void Dispose() => ((IDisposable)ate).Dispose();

        public void UpdateForBattle(Vector3 pos) =>
            //if used beyond battle might need to update this to use different matrices

            //Viewport vp = Memory.graphics.GraphicsDevice.Viewport;
            //var _1 = vp.Unproject(new Vector3(vp.Width / 2f, vp.Height / 2f, 0f), Module_battle_debug.ProjectionMatrix, Module_battle_debug.ViewMatrix, Module_battle_debug.WorldMatrix);
            //var _2 = vp.Unproject(new Vector3(vp.Width / 2f, vp.Height / 2f, 1f), Module_battle_debug.ProjectionMatrix, Module_battle_debug.ViewMatrix, Module_battle_debug.WorldMatrix);
            //var centerscreen = Vector3.Normalize(_2 - _1);
            //Matrix t = Matrix.CreateTranslation(pos);
            //Matrix lookat = Matrix.CreateLookAt(pos, _1, Vector3.Up);

            //float yaw = (float)Math.Atan2(lookat.M13, lookat.M33);
            //float pitch = (float)Math.Asin(-lookat.M23);
            //float roll = (float)Math.Atan2(lookat.M21, lookat.M22);

            //Quaternion q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0f);

            //The above code does the same thing as CreateBillboard but if you need more control might be worth having.
            Update(Module_battle_debug.CreateBillboard(pos));

        public void Update(Matrix bb)
        {
            for (int i = 0; i < VPT.Length; i++)
            {
                TransformedVPT[i].Position = Vector3.Transform(VPT[i].Position, bb);
                //TransformedVPT[i].Position = Vector3.Transform(VPT[i].Position, q);
                //TransformedVPT[i].Position = Vector3.Transform(TransformedVPT[i].Position, t);
            }
        }

        public void DrawForBattle()
        {
            ate.World = Module_battle_debug.WorldMatrix;
            ate.View = Module_battle_debug.ViewMatrix;
            ate.Projection = Module_battle_debug.ProjectionMatrix;
            Draw();
        }

        public void Draw()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Module_battle_debug.Effect.TextureEnabled = true;

            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                vertexData: TransformedVPT, vertexOffset: 0, primitiveCount: VPT.Length / 3);
            }
        }

        #endregion Methods
    }
}