using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.World
{
    class SpecialEffectsRenderer
    {
        //FX SYSTEM EXPLAINED
        /*
         * So in vanilla ff8 when you move through the forest for example it spawns the texture
         * and that texture shrinks to 0% then is destroyed.
         * The fxWalkDuration should be incremented every bHasMoved until X then spawn texture at player position
         * with 100% scale and spriteId until it gets to 0% and is destroyed
         *
         */
        private static float fxWalkDuration = 0;
        private static int lastFxBushSpriteId = 0;
        struct worldFx
        {
            public Vector3 fxLocation;
            public float scale;
            public int atlasId;
        }
        private static List<worldFx> worldEffects = new List<worldFx>();
        /// <summary>
        /// Takes care of drawing shadows and additional FX when needed (like in forest). [WIP]
        /// </summary>
        public static void DrawCharacterShadowSpecialEffects()
        {
           Module_world_debug.worldCharacterInstances[Module_world_debug.currentControllableEntity].bDraw = true; //always draw and later test the cases
            if (Module_world_debug.activeCollidePolygon == null)
                return;

            #region casual shadow

            if ((Module_world_debug.activeCollidePolygon.Value.vertFlags & Module_world_debug.TRIFLAGS_FORESTTEST) > 0 && Module_world_debug.activeCollidePolygon.Value.groundtype > 5) //shadow
            {
                var shadowGeom = Extended.GetShadowPlane(Module_world_debug.playerPosition + new Vector3(-2.2f, .1f, -2.2f), 4f);
                Module_world_debug.ate.Texture = (Texture2D)Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.shadowBig, 0);
                Module_world_debug.ate.Alpha = .25f;
                foreach (var pass in Module_world_debug.ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Module_world_debug.ate.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                    Memory.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, shadowGeom, 0, shadowGeom.Length / 3);
                }
                Module_world_debug.ate.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Module_world_debug.ate.Alpha = 1f;
            }
            #endregion
            #region forest leaves fx
            else if ((Module_world_debug.activeCollidePolygon.Value.vertFlags & Module_world_debug.TRIFLAGS_FORESTTEST) == 0 && Module_world_debug.activeCollidePolygon.Value.texFlags == 0
                && Module_world_debug.activeCollidePolygon.Value.groundtype <= 5) //forest
            {
                Module_world_debug.ate.Alpha = 1f;
                Module_world_debug.worldCharacterInstances[Module_world_debug.currentControllableEntity].bDraw = false;
                if (Module_world_debug.bHasMoved && fxWalkDuration > 0.25f)
                {
                    fxWalkDuration = 0f;
                    lastFxBushSpriteId %= 4;
                    worldEffects.Add(new worldFx()
                    {
                        fxLocation = Module_world_debug.playerPosition,
                        scale = 1.00f, atlasId = lastFxBushSpriteId++
                    });

                }
                else fxWalkDuration += 0.05f;
            }
            #endregion

            //All effects renderer below except shadows which are rendered in their own region

            for (int i = worldEffects.Count - 1; i > 0; i--)
            {
                //we get basic square geometry here
                VertexPositionTexture[] shadowGeom = Extended.GetShadowPlane(worldEffects[i].fxLocation +
                   new Vector3(-2.2f, .1f, -2.2f), 12f * worldEffects[i].scale);

                Extended.ConvertToSprite(ref shadowGeom, 4, worldEffects[i].atlasId);

                Module_world_debug.ate.Texture = (Texture2D)Module_world_debug.wmset.GetWorldMapTexture(Wmset.Section38_textures.wmfx_bush,
                    MathHelper.Clamp(GetLeavesFxClut(Module_world_debug.activeCollidePolygon), 0, 5));
                Module_world_debug.ate.Alpha = 0.75f;
                foreach (EffectPass pass in Module_world_debug.ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Module_world_debug.ate.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                    Memory.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, shadowGeom, 0, shadowGeom.Length / 3);
                }
                //ate.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                worldFx currentFx = worldEffects[i];
                currentFx.scale -= 0.005f;
                if (currentFx.scale < 0.8)
                    worldEffects.RemoveAt(i);
                else
                    worldEffects[i] = currentFx;
            }
        }

        /// <summary>
        /// Gets leaves fx clut ID based on actively colliding polygon. The clut ID is tied to ground type.
        /// </summary>
        /// <param name="activeCollidePolygon">polygon that player is on</param>
        /// <returns></returns>
        private static int GetLeavesFxClut(Module_world_debug.Polygon? activeCollidePolygon)
        {
            switch (activeCollidePolygon.Value.groundtype)
            {
                case 0:
                    return 5;
                case 1:
                    return 3;
                case 2:
                    return 1;
                case 3:
                    return 4;
                case 4:
                    return 0;
                case 5:
                    return 2;
                default:
                    return 0;
            }
        }
    }
}
