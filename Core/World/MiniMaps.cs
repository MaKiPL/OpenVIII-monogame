using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.World
{
    class MiniMaps
    {

        //exe hardcode at FF82000:00C76BE8
        private static byte[] wm_planetMinimap_indicesA_quad = new byte[]
        {
            0x0f,0x10,0x17,0x18,0x07,0x08,0x0e,0x0f,0x08,0x09,0x0f,0x10,0x09,0x0a,0x10,0x11,0x0e,0x0f,0x16,0x17,0x10,0x11,0x18,0x19,0x16,0x17,0x1d,0x1e,0x17,0x18,0x1e,0x1f,0x18,0x19,0x1f,0x20,0x02,0x03,0x07,0x08,0x03,0x04,0x08,0x09,0x04,0x05,0x09,0x0a,0x06,0x07,0x0d,0x0e,0x0d,0x0e,0x15,0x16,0x15,0x16,0x1c,0x1d,0x0a,0x0b,0x11,0x12,0x11,0x12,0x19,0x1a,0x19,0x1a,0x20,0x21,0x1d,0x1e,0x22,0x23,0x1e,0x1f,0x23,0x24,0x1f,0x20,0x24,0x25,0x00,0x01,0x03,0x04,0x0c,0x0d,0x14,0x15,0x12,0x13,0x1a,0x1b,0x23,0x24,0x26,0x27
        };

        //exe hardcode at FF82000:00C76C4C
        private static byte[] wm_planetMinimap_indicesB_tris = new byte[]
        {
            0x00,0x02,0x03,0xff,0x01,0x04,0x05,0xff,0x06,0x0c,0x0d,0xff,0x0b,0x12,0x13,0xff,0x14,0x15,0x1c,0xff,0x1a,0x1b,0x21,0xff,0x22,0x23,0x26,0xff,0x24,0x25,0x27,0xff,0x02,0x06,0x07,0xff,0x05,0x0a,0x0b,0xff,0x1c,0x1d,0x22,0xff,0x20,0x21,0x25,0xff
        };

        //exe hardcode at FF82000:00C76C7C
        private static byte[] wm_planetMinimap_vertices = new byte[]
        {
            0xf8,0xdb,0x08,0xdb,0xe8,0xe2,0xf8,0xe2,0x08,0xe2,0x18,0xe2,0xe0,0xea,0xe8,0xea,0xf8,0xea,0x08,0xea,0x18,0xea,0x20,0xea,0xd8,0xf9,0xe0,0xf9,0xe8,0xf9,0xf8,0xf9,0x08,0xf9,0x18,0xf9,0x20,0xf9,0x28,0xf9,0xd8,0x07,0xe0,0x07,0xe8,0x07,0xf8,0x07,0x08,0x07,0x18,0x07,0x20,0x07,0x28,0x07,0xe0,0x16,0xe8,0x16,0xf8,0x16,0x08,0x16,0x18,0x16,0x20,0x16,0xe8,0x1e,0xf8,0x1e,0x08,0x1e,0x18,0x1e,0xf8,0x25,0x08,0x25
        };

        //exe hardcode at FF82000:00C76CCC
        private static byte[] wm_planetMinimap_uvsOffsets = new byte[]
        {
            0xfc,0xe0,0x04,0xe0,0xf0,0xe8,0xfc,0xe8,0x04,0xe8,0x10,0xe8,0xe8,0xf0,0xf0,0xf0,0xfc,0xf0,0x04,0xf0,0x10,0xf0,0x18,0xf0,0xe0,0xfc,0xe8,0xfc,0xf0,0xfc,0xfc,0xfc,0x04,0xfc,0x10,0xfc,0x18,0xfc,0x20,0xfc,0xe0,0x04,0xe8,0x04,0xf0,0x04,0xfc,0x04,0x04,0x04,0x10,0x04,0x18,0x04,0x20,0x04,0xe8,0x10,0xf0,0x10,0xfc,0x10,0x04,0x10,0x10,0x10,0x18,0x10,0xf0,0x18,0xfc,0x18,0x04,0x18,0x10,0x18,0xfc,0x20,0x04,0x20,0x00,0x00,0x00,0x00
        };

        public static void DrawPlanetMiniMap()
        {
            var vpt = new List<VertexPositionTexture>();

            var planetCamPos = new Vector3(2098.347f, 32.68309f, -244.1487f);
            var planetCamTarget = new Vector3(2099.964f, 34.26089f, -234.208243f);

            //2000,0,0 - target
            Module_world_debug.viewMatrix = Matrix.CreateLookAt(planetCamPos, planetCamTarget,
             Vector3.Up);
            Module_world_debug.effect.View = Module_world_debug.viewMatrix;
            Module_world_debug.effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60), Memory.Graphics.GraphicsDevice.Viewport.AspectRatio, 1, 10000f);


            for (var i = 0; i < wm_planetMinimap_indicesB_tris.Length; i++) //triangles are ABC, so we can just iterate one-by-one
            {
                var offsetPointer = wm_planetMinimap_indicesB_tris[i];
                if (offsetPointer == 0xFF)
                    continue;

                offsetPointer *= 2;

                var vertX = (sbyte)wm_planetMinimap_vertices[offsetPointer];
                var vertY = (sbyte)wm_planetMinimap_vertices[offsetPointer + 1];

                //uv
                short u = (sbyte)wm_planetMinimap_uvsOffsets[offsetPointer];
                short v = (sbyte)wm_planetMinimap_uvsOffsets[offsetPointer + 1];

                var UVu = Module_world_debug.playerPosition.X / -16384.0f + u / 100.0f;
                var UVv = Module_world_debug.playerPosition.Z / -12288.0f + v / 100.0f;


                var vec = new Vector3(-vertX + 2000f, -vertY, 0);
                vpt.Add(new VertexPositionTexture(vec, new Vector2(UVu, UVv)));
            }

            for (var i = 0; i < wm_planetMinimap_indicesA_quad.Length; i += 4) //ABD ACD -> we have to retriangulate it
            {
                Vector3 A = Vector3.Zero, B = Vector3.Zero, C = Vector3.Zero, D = Vector3.Zero;
                var uvA = Vector2.Zero; var uvB = Vector2.Zero; var uvC = Vector2.Zero; var uvD = Vector2.Zero;

                for (var n = 0; n < 4; n++)
                {
                    var offsetPointer = wm_planetMinimap_indicesA_quad[i + n];
                    offsetPointer *= 2;
                    var vertX = (sbyte)wm_planetMinimap_vertices[offsetPointer];
                    var vertY = (sbyte)wm_planetMinimap_vertices[offsetPointer + 1];

                    //uv
                    short u = (sbyte)wm_planetMinimap_uvsOffsets[offsetPointer];
                    short v = (sbyte)wm_planetMinimap_uvsOffsets[offsetPointer + 1];

                    var UVu = Module_world_debug.playerPosition.X / -16384.0f + u / 100.0f;
                    var UVv = Module_world_debug.playerPosition.Z / -12288.0f + v / 100.0f;

                    var vec = new Vector3(-vertX + 2000f, -vertY, 0);
                    var vecUV = new Vector2(UVu, UVv);
                    if (n == 0)
                    { A = vec; uvA = vecUV; }
                    if (n == 1)
                    { B = vec; uvB = vecUV; }
                    if (n == 2)
                    { C = vec; uvC = vecUV; }
                    if (n == 3)
                    { D = vec; uvD = vecUV; }
                }
                vpt.Add(new VertexPositionTexture(A, uvA));
                vpt.Add(new VertexPositionTexture(B, uvB));
                vpt.Add(new VertexPositionTexture(D, uvD));

                vpt.Add(new VertexPositionTexture(A, uvA));
                vpt.Add(new VertexPositionTexture(C, uvC));
                vpt.Add(new VertexPositionTexture(D, uvD));

            }

            foreach (var pass in Module_world_debug.effect.CurrentTechnique.Passes)
            {
                Module_world_debug.effect.Texture = (Texture2D)Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.worldmapMinimap, 0);
                pass.Apply();
                Module_world_debug.effect.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                Memory.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpt.ToArray(), 0, vpt.Count / 3);
            }

            var src = new Rectangle(Point.Zero, Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapPointer, 0).Size.ToPoint());
            Vector2 Scale = Memory.Scale(src.Width, src.Height, ScaleMode.FitBoth);
            src.Height = (int)((src.Width * Scale.X) / 30);
            src.Width = (int)((src.Height * Scale.Y) / 30);
            var dst = new Rectangle(
                (int)(Memory.Graphics.GraphicsDevice.Viewport.Width / 1.24f),
                (int)((float)Memory.Graphics.GraphicsDevice.Viewport.Height / 1.3f),
                src.Width,
                src.Height);

            //Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            Memory.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapPointer, 0).Draw(dst, Color.White * 1f, Module_world_debug.degrees * 6.3f / 360f + 2.5f, Vector2.Zero, SpriteEffects.None, 1f);
            Memory.SpriteBatchEnd();


            Module_world_debug.effect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //restore matrices
            Module_world_debug.viewMatrix = Matrix.CreateLookAt(Module_world_debug.camPosition, Module_world_debug.camTarget,
                Vector3.Up);
            Module_world_debug.effect.View = Module_world_debug.viewMatrix;

        }

        /// <summary>
        /// Draws rectangle mini map
        /// </summary>
        public static void DrawRectangleMiniMap()
        {
            var src = new Rectangle(Point.Zero, Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.worldmapMinimap, 1).Size.ToPoint());
            Vector2 Scale = Memory.Scale(src.Width, src.Height, ScaleMode.FitBoth);
            src.Width = (int)(src.Width * Scale.X);
            src.Height = (int)(src.Height * Scale.Y);
            src.Height /= 2;
            src.Width /= 2;
            var dst =
                new Rectangle(
                    Memory.Graphics.GraphicsDevice.Viewport.Width - (src.Width) - 50,
                    Memory.Graphics.GraphicsDevice.Viewport.Height - (src.Height) - 50,
                    src.Width,
                    src.Height);

            var bc = Math.Abs(Module_world_debug.camPosition.X / 16384.0f);
            var topX = dst.X + (dst.Width * bc);
            var bd = Math.Abs(Module_world_debug.camPosition.Z / 12288f);
            var topY = dst.Y + (dst.Height * bd);

            //Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.worldmapMinimap, 1)
                .Draw(dst, Color.White * .7f);
            Memory.SpriteBatch.End();


            //Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            Memory.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            src = new Rectangle(Point.Zero, 
                Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapPointer, 0).Size.ToPoint());
            Scale = Memory.Scale(src.Width, src.Height, ScaleMode.FitBoth);
            src.Height = (int)((src.Width * Scale.X) / 30);
            src.Width = (int)((src.Height * Scale.Y) / 30);
            var dst2 = new Rectangle(
                (int)topX,
                (int)topY,
                src.Width,
                src.Height);
            Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapPointer, 0)
                .Draw(dst2, 
                Color.White * 1f, 
                Module_world_debug.degrees * 6.3f / 360f + 2.5f, 
                Vector2.Zero, 
                SpriteEffects.None, 
                1f);

            float localRotation = MathHelper.ToDegrees(
                Module_world_debug.worldCharacterInstances[Module_world_debug.currentControllableEntity].localRotation);
            if (localRotation < 0) localRotation += 360f;
            src = new Rectangle(Point.Zero, 
                Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapGunbladePointer, 0).Size.ToPoint());
            Scale = Memory.Scale(src.Width, src.Height, ScaleMode.FitBoth);
            src.Height = (int)((src.Width * Scale.X) / 30);
            src.Width = (int)((src.Height * Scale.Y) / 30);
            topX = dst2.X;// + (dst2.Width * bc);
            topY = dst2.Y;// + (dst2.Height * bd);
            dst = new Rectangle(
                (int)topX,
                (int)topY,
                (int)src.Width,
                (int)src.Height);
            Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.minimapGunbladePointer, 0).Draw(
                dst, 
                Color.White * 1f, 
                (-localRotation + 90f) * 6.3f / 360f, 
                new Vector2(8, 8), 
                SpriteEffects.None, 1f);
            Memory.SpriteBatchEnd();
        }




        private static float fulscrMapCurX = 0.5f;
        private static float fulscrMapCurY = 0.4f;


        struct fullScreenMapLocations
        {
            public bool bDraw;
            public float x;
            public float y;
            public FF8String locationName;
        }

        private static fullScreenMapLocations[] screenMapLocations = new fullScreenMapLocations[19]
        {
            new fullScreenMapLocations() {x= 0.555f, y= 0.345f}, //BGarden
            new fullScreenMapLocations() {x= 0.530f, y= 0.370f}, //BCity
            new fullScreenMapLocations() {x= 0.465f, y= 0.315f}, //Dollet
            new fullScreenMapLocations() {x= 0.449f, y= 0.455f}, //Timber
            new fullScreenMapLocations() {x= 0.410f, y= 0.370f}, //GGarden
            new fullScreenMapLocations() {x= 0.360f, y= 0.350f}, //Deling
            new fullScreenMapLocations() {x= 0.375f, y= 0.450f}, //Desert Prison
            new fullScreenMapLocations() {x= 0.340f, y= 0.415f}, //Missile Base
            new fullScreenMapLocations() {x= 0.542f, y= 0.475f}, //Fisherman
            new fullScreenMapLocations() {x= 0.385f, y= 0.510f}, //Winhill
            new fullScreenMapLocations() {x= 0.430f, y= 0.760f}, //Edea House
            new fullScreenMapLocations() {x= 0.611f, y= 0.240f}, //Trabia
            new fullScreenMapLocations() {x= 0.525f, y= 0.14f}, //Shumi
            new fullScreenMapLocations() {x= 0.640f, y= 0.450f}, //Esthar city
            new fullScreenMapLocations() {x= 0.620f, y= 0.485f}, //Esthar airstation
            new fullScreenMapLocations() {x= 0.687f, y= 0.460f}, //Lunatic pandora
            new fullScreenMapLocations() {x= 0.715f, y= 0.51f}, //Lunar gate
            new fullScreenMapLocations() {x= 0.675f, y= 0.535f}, //Esthar memorial
            new fullScreenMapLocations() {x= 0.698f, y= 0.590f}, //Tear point
        };
        //0.145 - 0.745 X
        //0.070 - 0.870 Y
        private static bool bFullScreenMapInitialize = true;
        private static Texture2D fullScreenMapMark;

        public static void DrawFullScreenMap()
        {
            if (bFullScreenMapInitialize)
            {
                screenMapLocations[0].locationName = Module_world_debug.wmset.GetLocationName(0); //BalambGarden
                screenMapLocations[0].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[1].locationName = Module_world_debug.wmset.GetLocationName(1); //Balamb City
                screenMapLocations[1].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[2].locationName = Module_world_debug.wmset.GetLocationName(3); //DOllet
                screenMapLocations[2].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[3].locationName = Module_world_debug.wmset.GetLocationName(4); //Timber
                screenMapLocations[3].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[4].locationName = Module_world_debug.wmset.GetLocationName(6); //Galbadia Garden
                screenMapLocations[4].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[5].locationName = Module_world_debug.wmset.GetLocationName(8); //Deling
                screenMapLocations[5].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[6].locationName = Module_world_debug.wmset.GetLocationName(10); //Desert Prison
                screenMapLocations[6].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[7].locationName = Module_world_debug.wmset.GetLocationName(11); //Missile Base
                screenMapLocations[7].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[8].locationName = Module_world_debug.wmset.GetLocationName(13); //Fisherman
                screenMapLocations[8].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[9].locationName = Module_world_debug.wmset.GetLocationName(15); //Winhill
                screenMapLocations[9].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[10].locationName = Module_world_debug.wmset.GetLocationName(18); //Edea house
                screenMapLocations[10].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[11].locationName = Module_world_debug.wmset.GetLocationName(19); //Trabia
                screenMapLocations[11].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[12].locationName = Module_world_debug.wmset.GetLocationName(20); //shumi
                screenMapLocations[12].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[13].locationName = Module_world_debug.wmset.GetLocationName(26); //Esthar city
                screenMapLocations[13].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[14].locationName = Module_world_debug.wmset.GetLocationName(27); //Esthar airstation
                screenMapLocations[14].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[15].locationName = Module_world_debug.wmset.GetLocationName(28); //Lunatic pandora lab
                screenMapLocations[15].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[16].locationName = Module_world_debug.wmset.GetLocationName(29); //lunar gate
                screenMapLocations[16].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[17].locationName = Module_world_debug.wmset.GetLocationName(30); //esthar memorial
                screenMapLocations[17].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS
                screenMapLocations[18].locationName = Module_world_debug.wmset.GetLocationName(31); //tear point
                screenMapLocations[18].bDraw = true; //[TODO] SAVEGAME PARSE FLAGS

                fullScreenMapMark = new Texture2D(Memory.Graphics.GraphicsDevice, 2, 1, false, SurfaceFormat.Color); //1x1 yellow and 1x1 red
                fullScreenMapMark.SetData(new Color[] { Color.Yellow, Color.Red });

                bFullScreenMapInitialize = false;
            }
            //Draw full-screen minimap
            Memory.Graphics.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchStartStencil();
            var texture = Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.worldmapMinimap, 0);
            var width = Memory.Graphics.GraphicsDevice.Viewport.Width;
            var height = Memory.Graphics.GraphicsDevice.Viewport.Height;
            texture.Draw(new Rectangle((int)(width * 0.2f), (int)(height * 0.08f),
                (int)(width * 0.6), (int)(height * 0.8)), Color.White * 1f);
            Memory.SpriteBatchEnd();

            Memory.SpriteBatchStartAlpha();
            //draw locations
            for (var i = 0; i < screenMapLocations.Length; i++)
            {
                if (!screenMapLocations[i].bDraw)
                    continue;
                Memory.SpriteBatch.Draw(fullScreenMapMark, new Rectangle((int)(width * screenMapLocations[i].x),
                    (int)(height * screenMapLocations[i].y), 8, 8),
                    new Rectangle(0, 0, 1, 1), Color.White);
            }
            //draw vehicles
            //[TODO]
            //draw location names
            //[TODO]
            for (var i = 0; i < screenMapLocations.Length; i++)
            {
                var xDistance = Math.Abs((fulscrMapCurX + 0.05f) - screenMapLocations[i].x);
                var yDistance = Math.Abs((fulscrMapCurY + 0.005f) - screenMapLocations[i].y);
                if (xDistance < 0.015 && yDistance < 0.015)
                    Memory.Font.RenderBasicText(screenMapLocations[i].locationName,
                        new Vector2(width * 0.7f, height * 0.9f), new Vector2(2, 2), Font.Type.sysFntBig);
            }
            Memory.SpriteBatchEnd();

            //Finally draw cursor
            Memory.SpriteBatchStartAlpha();
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2,
                new Rectangle((int)(fulscrMapCurX * width), (int)(fulscrMapCurY * height), 0, 0), Vector2.One * 3);

            Memory.SpriteBatchEnd();
        }

        internal static void Input()
        {
            if (Input2.Button(FF8TextTagKey.Up)/* || shift.Y > 0*/)
            {
                if (fulscrMapCurY < 0.070f)
                    fulscrMapCurY = 0.870f;
                fulscrMapCurY -= 0.005f * (float)Memory.ElapsedGameTime.TotalMilliseconds / 25.0f;
            }
            else if (Input2.Button(FF8TextTagKey.Down)/* || shift.Y < 0*/)
            {
                if (fulscrMapCurY > 0.870f)
                    fulscrMapCurY = 0.070f;
                fulscrMapCurY += 0.005f * (float)Memory.ElapsedGameTime.TotalMilliseconds / 25.0f;

            }
            if (Input2.Button(FF8TextTagKey.Left) /*|| shift.X < 0*/)
            {
                if (fulscrMapCurX < 0.145f)
                    fulscrMapCurX = 0.745f;
                fulscrMapCurX -= 0.003f * (float)Memory.ElapsedGameTime.TotalMilliseconds / 25.0f;

            }
            else if (Input2.Button(FF8TextTagKey.Right)/* || shift.X > 0*/)
            {
                if (fulscrMapCurX > 0.745)
                    fulscrMapCurX = 0.145f;
                fulscrMapCurX += 0.003f * (float)Memory.ElapsedGameTime.TotalMilliseconds / 25.0f;
            }
        }

        internal static void imgui()
        {
            ImGuiNET.ImGui.InputFloat("X: ", ref fulscrMapCurX); //0.145 - 0.745
            ImGuiNET.ImGui.InputFloat("Y: ", ref fulscrMapCurY); //0.070 - 0.870
            for (var i = 0; i < screenMapLocations.Length; i++)
            {
                ImGuiNET.ImGui.InputFloat($"lA{i}", ref screenMapLocations[i].x);
                ImGuiNET.ImGui.SameLine();
                ImGuiNET.ImGui.InputFloat($"lB{i}", ref screenMapLocations[i].y);
            }
        }
    }
}
