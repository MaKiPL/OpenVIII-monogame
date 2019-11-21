using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    public static class BattleSwirl
    {
        static Texture2D backBufferTexture;
        private static bool bInitialized = false;
        private static bool bFirstClear = true;
        private static int swirlMode = 0;

        private static double sequenceTimer = 0.0;
        private static double activeFrameTimer = 0.0;

        public static void Init()
        {
            bInitialized = false;
            //Memory.encounters[Memory.battle_encounter].BattleFlags. //which flags is about the boss or normal swirl?
            Memory.module = MODULE.BATTLE_SWIRL;
            backBufferTexture = Extended.BackBufferTexture;
            bInitialized = true;
            bFirstClear = true;
            sequenceTimer = 0.0;
            activeFrameTimer = 0.0;
            scaleModifier = 1.0f; //maybe create this swirl as non-static class?
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
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            Memory.font.RenderBasicText(
                $"sequence: ={sequenceTimer}", 30, 20, 1f, 2f, lineSpacing: 5);
            //Memory.spriteBatch.Draw(backBufferTexture, new Rectangle(0, 0, backBufferTexture.Width, backBufferTexture.Height), Color.White*0.2f);
            if (activeFrameTimer > 1000.0 / 500.0)
            {
                activeFrameTimer = 0.0;
                scaleModifier += 0.003f;
            switch (swirlMode)
            {
                case 0:
                    NormalSwirlDraw();
                    break;
                case 1:
                    BossSwirlDraw();
                    break;
            };
            }
            Memory.SpriteBatchEnd();
        }


        static float scaleModifier = 1f;

        private static void NormalSwirlDraw()
        {
            if (sequenceTimer < 0.50) //frame repeat by scaling on un-cleaned buffer
            {
                Vector2 resolution = new Vector2(backBufferTexture.Width, backBufferTexture.Height);
                var transformedScale = Vector2.Transform(resolution, Matrix.CreateScale(scaleModifier));
                Memory.spriteBatch.Draw(backBufferTexture, new Rectangle((int)(transformedScale.X - resolution.X) / -3
                    , (int)(transformedScale.Y - resolution.Y) / -3
                    , (int)transformedScale.X, (int)transformedScale.Y),
                    Color.White * 0.15f);
            }
            if(sequenceTimer > 0.50) //black lines going from left (grayscale 255 transition mask-just like in RPG Maker) [but there's no texture?]
            {
                
            }
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
