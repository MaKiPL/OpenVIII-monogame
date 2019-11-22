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
    /// it's currently in mess, but I will clean it after I got everything working and comment things
    /// </summary>
    public static class BattleSwirl
    {
        static Texture2D backBufferTexture;
        private static bool bInitialized = false;
        private static bool bFirstClear = true;
        private static int swirlMode = 0;
        private static int swirlStage = 0;

        private static double sequenceTimer = 0.0;
        private static double activeFrameTimer = 0.0;

        static Texture2D blackLines;

        public static void Init()
        {
            bInitialized = false;
            //Memory.encounters[Memory.battle_encounter].BattleFlags. //which flags is about the boss or normal swirl?
            Memory.module = MODULE.BATTLE_SWIRL;
            backBufferTexture = Extended.BackBufferTexture;
            bInitialized = true;
            bFirstClear = true;
            swirlStage = 0;
            sequenceTimer = 0.0;
            activeFrameTimer = 0.0;
            scaleModifier = 1.0f; //maybe create this swirl as non-static class?
            translateModifier = 0.0f;

            //let's now generate the black lines screen cleaning buffer
            Random random = new Random();
            blackLines = new Texture2D(Memory.graphics.GraphicsDevice, 1,
                                                 Memory.graphics.GraphicsDevice.Viewport.Height, false,
                                                 SurfaceFormat.Color);
            Color[] colors = new Color[blackLines.Height];
            for(int i = 0; i<blackLines.Height; i++)
            {
                int lineBuffer = random.Next(0, 120);
                colors[i] = new Color(Color.Black, lineBuffer);
            }
            blackLines.SetData(colors);
        }

        //because of the backBuffer;
        public static void ContinueStage()
        {
            backBufferTexture = Extended.BackBufferTexture;
            swirlStage++;
        }

        //private static void BossSwirl()
        //{
        //    return;
        //}

        //private static void NormalSwirl()
        //{
        //    return;
        //}

        public static void Draw()
        {
            if (!bInitialized)
                return;
            sequenceTimer += Memory.gameTime.ElapsedGameTime.TotalMilliseconds/1000.0;
            activeFrameTimer += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
            //Memory.graphics.GraphicsDevice.Clear(Color.Black);
            if (activeFrameTimer > 1000.0 / 500.0)
            {
                activeFrameTimer = 0.0;
            switch (swirlMode)
            {
                case 0:
                        if (swirlStage == 0)
                            NormalSwirlDraw_stage1();
                        else
                            NormalSwirlDraw_stage2();
                    break;
                case 1:
                    BossSwirlDraw();
                    break;
            };
            }
        }


        static float scaleModifier = 1f;
        static float translateModifier = 0f;

        private static void NormalSwirlDraw_stage1()
        {
            if (sequenceTimer < 0.50) //frame repeat by scaling on un-cleaned buffer
            {
            scaleModifier += 0.003f;
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
                Vector2 resolution = new Vector2(backBufferTexture.Width, backBufferTexture.Height);
                var transformedScale = Vector2.Transform(resolution, Matrix.CreateScale(scaleModifier));
                Memory.spriteBatch.Draw(backBufferTexture, new Rectangle((int)(transformedScale.X - resolution.X) / -3
                    ,0 /*(int)(transformedScale.Y - resolution.Y) / -3*/
                    , (int)transformedScale.X, (int)transformedScale.Y),
                    Color.White * 0.15f);
                Memory.spriteBatch.End();
            }
            if(sequenceTimer > 0.50) //black lines going from left (grayscale 127 transition mask-just like in RPG Maker)
            {
                Extended.postBackBufferDelegate = ContinueStage;
                Extended.RequestBackBuffer();
            }
        }

        //wip
        private static void NormalSwirlDraw_stage2()
        {
            //it's a bit more complicated- tbh I didn't reverse fully the algorithm, but I know the buffer is generated for screenY 
            //the buffer is int32, but normally it's capped to 120. The buffer is generate before the swirl with FFSwirlInitModule
            //now it works like this- the texture is drawn from left to right by 'clearing' the screen. 
            //value of 00 means instant black, while any bigger value means obviously 'slower' clearing




            //actually it looks like the delay of line clearing; probably this value is time. So the texture is cleared by delay <------------






            //int x = (int)(Memory.graphics.GraphicsDevice.Viewport.Width * translateModifier);
            int x = (int)translateModifier;
            translateModifier += 1f;
            //translateModifier += 0.002f;
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque);
            Memory.spriteBatch.Draw(backBufferTexture, 
                new Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                Color.White * 1f);

            Memory.spriteBatch.Draw(blackLines, new Rectangle(x
                , 0
                , 1, Memory.graphics.GraphicsDevice.Viewport.Height),
                Color.White * 1f);
            Memory.spriteBatch.End();
        }

        private static void BossSwirlDraw()
        {
            //here is two screens going with 0.50 scaling from x to y and back to x repeating themselves up to the point
            //where additive rendering creates white screen - probably rising color.White * n [where n>1]
            //this probably uses cleaned render target
            return;
        }

    }
}
