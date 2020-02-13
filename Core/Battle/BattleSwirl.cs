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
            //Memory.Encounters.Current().BattleFlags. //which flags is about the boss or normal swirl?
            Memory.Module = MODULE.BATTLE_SWIRL;
            backBufferTexture = Extended.BackBufferTexture;
            bInitialized = true;
            bFirstClear = true;
            swirlStage = 0;
            sequenceTimer = 0.0;
            activeFrameTimer = 0.0;
            scaleModifier = 1.0f; //maybe create this swirl as non-static class?
            translateModifier = 0.0f;

            //let's now generate the black lines screen cleaning buffer
            blackLines = new Texture2D(Memory.graphics.GraphicsDevice, 0xFF + 120, //ok, so 0xff to fade alpha and 120 for maximum delay
                                                 Memory.graphics.GraphicsDevice.Viewport.Height, false,
                                                 SurfaceFormat.Color);
           Color[] colors = new Color[blackLines.Height*blackLines.Width];
            for (int i = 0; i < blackLines.Height; i++)
            {
                //we are incrementing by line- full width is 0xff+120
                int lineBuffer = Memory.Random.Next(0, 120); //generate the delay
                int scanAlpha = 255;
                for(int n=0; n<blackLines.Width-lineBuffer; n++) //fill with black
                    colors[i*blackLines.Width+n] = new Color(Color.Black, 0xFF);
                for (int n = lineBuffer; n < blackLines.Width; n++) //fill with fading black
                    colors[i * blackLines.Width + n] = new Color(0, 0, 0, scanAlpha--); //debug for red
            }
            blackLines.SetData(colors);
            Extended.DumpTexture(blackLines, @"D:\out.png");
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
            sequenceTimer += Memory.ElapsedGameTime.TotalMilliseconds/1000.0;
            activeFrameTimer += Memory.ElapsedGameTime.TotalMilliseconds;
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

        //wip - there's actually no need to do the second stage- clean this afterward
        private static void NormalSwirlDraw_stage2()
        {
            //int x = (int)(Memory.graphics.GraphicsDevice.Viewport.Width * translateModifier);
            int x = (int)translateModifier;
            translateModifier += 2f;
            //translateModifier += 0.002f;
            //Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //Memory.spriteBatch.Draw(backBufferTexture, 
            //    new Rectangle(0, 0, Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
            //    Color.White * 1f);

            Memory.spriteBatch.Draw(blackLines, new Rectangle(x-Memory.graphics.GraphicsDevice.Viewport.Width
                , 0
                , Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height),
                Color.White * 1f);
            Memory.spriteBatch.End();
            if (x > Memory.graphics.GraphicsDevice.Viewport.Width+blackLines.Width+0xff)
                Memory.Module = MODULE.BATTLE_DEBUG;
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
