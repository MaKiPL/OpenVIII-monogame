using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// this works only as a preview for field models and proof-of-concept
    /// </summary>
    public class module_field_object_test
    {
        private static int lastFieldId = -1;

        static int debugModelId = 0;
        static int animId = 0;
        static int animFrame = 0;

        static float timer = 0.0f;

        static FieldCharaOne charaOne;

        private static FPS_Camera fps_camera;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees;
        private static float camDistance = 10.0f;
        private static float renderCamDistance = 1200f;
        private static Vector3 camPosition, camTarget;
        public static BasicEffect effect;
        public static AlphaTestEffect ate;

        static bool bInitialized = false;

        public static void Update()
        {
            if(!bInitialized)
            {
                fps_camera = new FPS_Camera();
                //init renderer
                effect = new BasicEffect(Memory.graphics.GraphicsDevice);
                effect.EnableDefaultLighting();
                effect.TextureEnabled = true;
                effect.DirectionalLight0.Enabled = true;
                effect.DirectionalLight1.Enabled = false;
                effect.DirectionalLight2.Enabled = false;
                effect.DirectionalLight0.Direction = new Vector3(
                   -0.349999f,
                    0.499999f,
                    -0.650000f
                    );
                effect.DirectionalLight0.SpecularColor = new Vector3(0.8500003f, 0.8500003f, 0.8500003f);
                effect.DirectionalLight0.DiffuseColor = new Vector3(1.54999f, 1.54999f, 1.54999f);
                camTarget = new Vector3(0, 0f, 0f);
                camPosition = new Vector3(0, 0f, 0f);
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                                   MathHelper.ToRadians(60),
                                   Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                    1f, 10000f);
                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                             new Vector3(0f, 1f, 0f));// Y up
                                                      //worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                                                      //              Forward, Vector3.Up);
                worldMatrix = Matrix.CreateTranslation(0, 0, 0);
                //temporarily disabling this, because I'm getting more and more tired of this music playing over and over when debugging
                //Memory.musicIndex = 30;
                //init_debugger_Audio.PlayMusic();
                ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
                {
                    Projection = projectionMatrix,
                    View = viewMatrix,
                    World = worldMatrix,
                    FogEnabled = false,
                    FogColor = Color.CornflowerBlue.ToVector3(),
                    FogStart = 9.75f,
                    FogEnd = renderCamDistance
                };

                bInitialized = true;
            }
            if (lastFieldId != Memory.FieldHolder.FieldID)
                ReInit();
            viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
            if (Input2.Button(MouseButtons.LeftButton, ButtonTrigger.OnRelease))
                debugModelId++;
            if (Input2.Button(Microsoft.Xna.Framework.Input.Keys.F1, ButtonTrigger.OnRelease))
                ReInit();
            if (Input2.Button(Microsoft.Xna.Framework.Input.Keys.F2, ButtonTrigger.OnPress))
                Memory.FieldHolder.FieldID++;
            if (Input2.Button(Microsoft.Xna.Framework.Input.Keys.F3, ButtonTrigger.OnPress))
                Memory.FieldHolder.FieldID--;
            if (Input2.Button(Microsoft.Xna.Framework.Input.Keys.F4, ButtonTrigger.OnPress))
            {
                animId++;
                animFrame = 0;
            }

            timer += (float)Memory.gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            if (timer > 0.033f)
            {
                animFrame++;
                timer = 0f;
            }
        }


        private static void ReInit()
        {
            lastFieldId = Memory.FieldHolder.FieldID;
            
            charaOne = new FieldCharaOne(Memory.FieldHolder.FieldID);
        }

        public static void Draw()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Memory.graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Aqua);
            if (!bInitialized)
                return;
            uint maxAnim = 0;
            uint maxFrame = 0;

            ate.Projection = projectionMatrix;
            ate.View = viewMatrix;
            ate.World = worldMatrix;
            effect.Projection = projectionMatrix;
            effect.View = viewMatrix;
            effect.World = worldMatrix;

            if (charaOne.fieldModels == null)
                goto _donotdraw;

            if (debugModelId >= charaOne.fieldModels.Length)
                debugModelId = 0;
            int whichModel = debugModelId;

            if (charaOne.fieldModels[whichModel].mch == null)
                goto _donotdraw;

            charaOne.fieldModels[whichModel].mch.AssignTextureSizes(
                charaOne.fieldModels[whichModel].textures, 
                Enumerable.Range(0,charaOne.fieldModels[whichModel].textures.Length).ToArray());

            maxAnim = charaOne.fieldModels[whichModel].mch.GetAnimationCount();
            if (animId >= maxAnim)
                animId = 0;
            maxFrame = charaOne.fieldModels[whichModel].mch.GetAnimationFramesCount(animId);
            if (animFrame >= maxFrame)
                animFrame = 0;

            Tuple<VertexPositionColorTexture[], byte[]> charaCollection = 
                charaOne.fieldModels[whichModel].mch.GetVertexPositions(Vector3.Zero,Quaternion.Identity, animId, animFrame);

            Dictionary<Texture2D, List<VertexPositionColorTexture>> vptCollection = new Dictionary<Texture2D, List<VertexPositionColorTexture>>();
            for (int i = 0; i < charaCollection.Item2.Length; i += 3)
            {
                Texture2D charaTexture = charaOne.fieldModels[whichModel].textures[charaCollection.Item2[i]];
                if (!vptCollection.ContainsKey(charaTexture))
                    vptCollection.Add(charaTexture, new List<VertexPositionColorTexture>());
                vptCollection[charaTexture].AddRange(charaCollection.Item1.Skip(i).Take(3).ToArray());
            }

            foreach (KeyValuePair<Texture2D, List<VertexPositionColorTexture>> kvp in vptCollection)
            {
                ate.Texture = kvp.Key;
                foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, kvp.Value.ToArray(), 0, kvp.Value.Count / 3);
                }
            }

            _donotdraw:
            Memory.SpriteBatchStartAlpha();
            if(charaOne.fieldModels == null)
            {
                Memory.font.RenderBasicText(
    $"FIELD AT: {Memory.FieldHolder.FieldID}\n" +
    $"World Map Camera: ={camPosition}\n" +
    $"FPS camera degrees: ={degrees}°\n" +
    $"Current model is: =BROKEN\n" +
    $"Animation={animId + 1} of {maxAnim} -- frame: {animFrame + 1} of {maxFrame}\n" +
    $"F1 - reinit (for reparsing and live code debugging)\n" +
    $"F2 - Next field\n" +
    $"F3 - Previous field\n" +
    $"F4 - Next animation\n" +
    $"LMB - Next NPC model\n" +
    $"NULL: ={0}", 30, 20, 1f, 2f, lineSpacing: 5);
            }
            else
                Memory.font.RenderBasicText(
    $"FIELD AT: {Memory.FieldHolder.FieldID}\n" +
    $"World Map Camera: ={camPosition}\n" +
    $"FPS camera degrees: ={degrees}°\n" +
    $"Current model is: ={debugModelId+1} of {charaOne.fieldModels.Length} which is {new string(charaOne.fieldModels[debugModelId].modelName,0,4)}\n" +
    $"Animation={animId+1} of {maxAnim} -- frame: {animFrame+1} of {maxFrame}\n" +
    $"F1 - reinit (for reparsing and live code debugging)\n" +
    $"F2 - Next field\n" +
    $"F3 - Previous field\n" +
    $"F4 - Next animation\n" +
    $"LMB - Next NPC model\n" +
    $"NULL: ={0}", 30, 20, 1f, 2f, lineSpacing: 5);
            Memory.SpriteBatchEnd();

        }
    }
}
