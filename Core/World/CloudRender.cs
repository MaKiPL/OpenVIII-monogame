using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.World
{
    class CloudRender
    {
        static Vector3[] wm_backgroundCylinderVerts = new Vector3[]
        {
new Vector3(41.8239f, 0.0000f, -3.9575f), new Vector3(36.2066f, 0.0000f, -24.9213f), new Vector3(20.8601f, 0.0000f, -40.2678f),
new Vector3(-0.1036f, 0.0000f, -45.8850f),new Vector3(-21.0674f, 0.0000f, -40.2678f),new Vector3(-36.4139f, 0.0000f, -24.9213f),
new Vector3(-42.0311f, 0.0000f, -3.9576f),new Vector3(-36.4139f, 0.0000f, 17.0062f),new Vector3(-21.0674f, 0.0000f, 32.3527f),
new Vector3(-0.1036f, 0.0000f, 37.9699f),new Vector3(20.8601f, 0.0000f, 32.3527f),new Vector3(36.2066f, 0.0000f, 17.0062f),
new Vector3(41.8239f, 13.4218f, -3.9575f),new Vector3(36.2066f, 13.4218f, -24.9213f),new Vector3(20.8601f, 13.4218f, -40.2678f),
new Vector3(-0.1036f, 13.4218f, -45.8850f),new Vector3(-21.0674f, 13.4218f, -40.2678f),new Vector3(-36.4139f, 13.4218f, -24.9213f),
new Vector3(-42.0311f, 13.4218f, -3.9576f),new Vector3(-36.4139f, 13.4218f, 17.0062f),new Vector3(-21.0674f, 13.4218f, 32.3527f),
new Vector3(-0.1036f, 13.4218f, 37.9699f),new Vector3(20.8601f, 13.4218f, 32.3527f),new Vector3(36.2066f, 13.4218f, 17.0062f)
        };

        static Vector2[] wm_backgroundCylinderVt = new Vector2[]
        {
new Vector2(2.5197f, -0.0005f),new Vector2(2.7717f, -0.0005f),new Vector2(3.0236f, -0.0005f),
new Vector2(0.2524f, -0.0005f),new Vector2(0.5043f, -0.0005f),new Vector2(0.7563f, -0.0005f),new Vector2(1.0082f, -0.0005f),
new Vector2(1.2601f, -0.0005f),new Vector2(1.5120f, -0.0005f),new Vector2(1.7640f, -0.0005f),new Vector2(2.0159f, -0.0005f),
new Vector2(2.2678f, -0.0005f),new Vector2(2.5197f, -0.9995f),new Vector2(2.7717f, -0.9995f),new Vector2(3.0236f, -0.9995f),
new Vector2(0.2524f, -0.9995f),new Vector2(0.5043f, -0.9995f),new Vector2(0.7563f, -0.9995f),new Vector2(1.0082f, -0.9995f),
new Vector2(1.2601f, -0.9995f),new Vector2(1.5120f, -0.9995f),new Vector2(1.7640f, -0.9995f),new Vector2(2.0159f, -0.9995f),
new Vector2(2.2678f, -0.9995f),new Vector2(0.0005f, -0.0005f),new Vector2(0.0005f, -0.9995f)
        };
        static VertexPositionTexture[] wm_backgroundCloudsCylinderMesh = new VertexPositionTexture[]
        {
new VertexPositionTexture(wm_backgroundCylinderVerts[0], wm_backgroundCylinderVt[0]),   new VertexPositionTexture(wm_backgroundCylinderVerts[12], wm_backgroundCylinderVt[12]), new VertexPositionTexture(wm_backgroundCylinderVerts[13], wm_backgroundCylinderVt[13]),
new VertexPositionTexture(wm_backgroundCylinderVerts[13], wm_backgroundCylinderVt[13]), new VertexPositionTexture(wm_backgroundCylinderVerts[1], wm_backgroundCylinderVt[1]),   new VertexPositionTexture(wm_backgroundCylinderVerts[0], wm_backgroundCylinderVt[0]),
new VertexPositionTexture(wm_backgroundCylinderVerts[1], wm_backgroundCylinderVt[1]),   new VertexPositionTexture(wm_backgroundCylinderVerts[13], wm_backgroundCylinderVt[13]), new VertexPositionTexture(wm_backgroundCylinderVerts[14], wm_backgroundCylinderVt[14]),
new VertexPositionTexture(wm_backgroundCylinderVerts[14], wm_backgroundCylinderVt[14]), new VertexPositionTexture(wm_backgroundCylinderVerts[2], wm_backgroundCylinderVt[2]),   new VertexPositionTexture(wm_backgroundCylinderVerts[1], wm_backgroundCylinderVt[1]),
new VertexPositionTexture(wm_backgroundCylinderVerts[2], wm_backgroundCylinderVt[24]),  new VertexPositionTexture(wm_backgroundCylinderVerts[14], wm_backgroundCylinderVt[25]), new VertexPositionTexture(wm_backgroundCylinderVerts[15], wm_backgroundCylinderVt[15]),
new VertexPositionTexture(wm_backgroundCylinderVerts[15], wm_backgroundCylinderVt[15]), new VertexPositionTexture(wm_backgroundCylinderVerts[3], wm_backgroundCylinderVt[3]),   new VertexPositionTexture(wm_backgroundCylinderVerts[2], wm_backgroundCylinderVt[24]),
new VertexPositionTexture(wm_backgroundCylinderVerts[3], wm_backgroundCylinderVt[3]),   new VertexPositionTexture(wm_backgroundCylinderVerts[15], wm_backgroundCylinderVt[15]), new VertexPositionTexture(wm_backgroundCylinderVerts[16], wm_backgroundCylinderVt[16]),
new VertexPositionTexture(wm_backgroundCylinderVerts[16], wm_backgroundCylinderVt[16]), new VertexPositionTexture(wm_backgroundCylinderVerts[4], wm_backgroundCylinderVt[4]),   new VertexPositionTexture(wm_backgroundCylinderVerts[3], wm_backgroundCylinderVt[3]),
new VertexPositionTexture(wm_backgroundCylinderVerts[4], wm_backgroundCylinderVt[4]),   new VertexPositionTexture(wm_backgroundCylinderVerts[16], wm_backgroundCylinderVt[16]), new VertexPositionTexture(wm_backgroundCylinderVerts[17], wm_backgroundCylinderVt[17]),
new VertexPositionTexture(wm_backgroundCylinderVerts[17], wm_backgroundCylinderVt[17]), new VertexPositionTexture(wm_backgroundCylinderVerts[5], wm_backgroundCylinderVt[5]),   new VertexPositionTexture(wm_backgroundCylinderVerts[4], wm_backgroundCylinderVt[4]),
new VertexPositionTexture(wm_backgroundCylinderVerts[5], wm_backgroundCylinderVt[5]),   new VertexPositionTexture(wm_backgroundCylinderVerts[17], wm_backgroundCylinderVt[17]), new VertexPositionTexture(wm_backgroundCylinderVerts[18], wm_backgroundCylinderVt[18]),
new VertexPositionTexture(wm_backgroundCylinderVerts[18], wm_backgroundCylinderVt[18]), new VertexPositionTexture(wm_backgroundCylinderVerts[6], wm_backgroundCylinderVt[6]),   new VertexPositionTexture(wm_backgroundCylinderVerts[5], wm_backgroundCylinderVt[5]),
new VertexPositionTexture(wm_backgroundCylinderVerts[6], wm_backgroundCylinderVt[6]),   new VertexPositionTexture(wm_backgroundCylinderVerts[18], wm_backgroundCylinderVt[18]), new VertexPositionTexture(wm_backgroundCylinderVerts[19], wm_backgroundCylinderVt[19]),
new VertexPositionTexture(wm_backgroundCylinderVerts[19], wm_backgroundCylinderVt[19]), new VertexPositionTexture(wm_backgroundCylinderVerts[7], wm_backgroundCylinderVt[7]),   new VertexPositionTexture(wm_backgroundCylinderVerts[6], wm_backgroundCylinderVt[6]),
new VertexPositionTexture(wm_backgroundCylinderVerts[7], wm_backgroundCylinderVt[7]),   new VertexPositionTexture(wm_backgroundCylinderVerts[19], wm_backgroundCylinderVt[19]), new VertexPositionTexture(wm_backgroundCylinderVerts[20], wm_backgroundCylinderVt[20]),
new VertexPositionTexture(wm_backgroundCylinderVerts[20], wm_backgroundCylinderVt[20]), new VertexPositionTexture(wm_backgroundCylinderVerts[8], wm_backgroundCylinderVt[8]),   new VertexPositionTexture(wm_backgroundCylinderVerts[7], wm_backgroundCylinderVt[7]),
new VertexPositionTexture(wm_backgroundCylinderVerts[8], wm_backgroundCylinderVt[8]),   new VertexPositionTexture(wm_backgroundCylinderVerts[20], wm_backgroundCylinderVt[20]), new VertexPositionTexture(wm_backgroundCylinderVerts[21], wm_backgroundCylinderVt[21]),
new VertexPositionTexture(wm_backgroundCylinderVerts[21], wm_backgroundCylinderVt[21]), new VertexPositionTexture(wm_backgroundCylinderVerts[9], wm_backgroundCylinderVt[9]),   new VertexPositionTexture(wm_backgroundCylinderVerts[8], wm_backgroundCylinderVt[8]),
new VertexPositionTexture(wm_backgroundCylinderVerts[9], wm_backgroundCylinderVt[9]),   new VertexPositionTexture(wm_backgroundCylinderVerts[21], wm_backgroundCylinderVt[21]), new VertexPositionTexture(wm_backgroundCylinderVerts[22], wm_backgroundCylinderVt[22]),
new VertexPositionTexture(wm_backgroundCylinderVerts[22], wm_backgroundCylinderVt[22]), new VertexPositionTexture(wm_backgroundCylinderVerts[10], wm_backgroundCylinderVt[10]), new VertexPositionTexture(wm_backgroundCylinderVerts[9], wm_backgroundCylinderVt[9]),
new VertexPositionTexture(wm_backgroundCylinderVerts[10], wm_backgroundCylinderVt[10]), new VertexPositionTexture(wm_backgroundCylinderVerts[22], wm_backgroundCylinderVt[22]), new VertexPositionTexture(wm_backgroundCylinderVerts[23], wm_backgroundCylinderVt[23]),
new VertexPositionTexture(wm_backgroundCylinderVerts[23], wm_backgroundCylinderVt[23]), new VertexPositionTexture(wm_backgroundCylinderVerts[11], wm_backgroundCylinderVt[11]), new VertexPositionTexture(wm_backgroundCylinderVerts[10], wm_backgroundCylinderVt[10]),
new VertexPositionTexture(wm_backgroundCylinderVerts[11], wm_backgroundCylinderVt[11]), new VertexPositionTexture(wm_backgroundCylinderVerts[23], wm_backgroundCylinderVt[23]), new VertexPositionTexture(wm_backgroundCylinderVerts[12], wm_backgroundCylinderVt[12]),
new VertexPositionTexture(wm_backgroundCylinderVerts[12], wm_backgroundCylinderVt[12]), new VertexPositionTexture(wm_backgroundCylinderVerts[0], wm_backgroundCylinderVt[0]),   new VertexPositionTexture(wm_backgroundCylinderVerts[11], wm_backgroundCylinderVt[11])
        };
        static VertexPositionTexture[] wm_backgroundCloudsLocalCylinderMeshTranslated = null;
        public static void DrawBackgroundClouds()
        {
            if (Module_world_debug.bHasMoved || wm_backgroundCloudsLocalCylinderMeshTranslated == null)
            {
                wm_backgroundCloudsLocalCylinderMeshTranslated = wm_backgroundCloudsCylinderMesh.Clone() as VertexPositionTexture[];
                for (var i = 0; i < wm_backgroundCloudsCylinderMesh.Length; i++)
                {
                    var pos = wm_backgroundCloudsCylinderMesh[i].Position;
                    pos = Vector3.Transform(pos,
                        Matrix.CreateScale(27f));
                    pos = Vector3.Transform(pos,
                        Matrix.CreateTranslation(new Vector3(Module_world_debug.playerPosition.X, -160, Module_world_debug.playerPosition.Z)));
                    wm_backgroundCloudsLocalCylinderMeshTranslated[i].Position = pos;
                }
            }


            Module_world_debug.ate.Texture = (Texture2D)Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.clouds, 0);
            foreach (var pass in Module_world_debug.ate.CurrentTechnique.Passes)
            {
                Module_world_debug.ate.FogEnd = 1500;
                Module_world_debug.ate.DiffuseColor = Vector3.One * 1.8f;
                Module_world_debug.ate.Alpha = 0.75f;
                pass.Apply();
                Memory.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, wm_backgroundCloudsLocalCylinderMeshTranslated, 0, wm_backgroundCloudsLocalCylinderMeshTranslated.Length / 3);
                Module_world_debug.ate.FogEnd = Module_world_debug.renderCamDistance;
            }
        }

    }
}
