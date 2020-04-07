using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    static class interchangeableZones
    {
        private enum interZone : int
        {
            prisonNormal = 834,
            prisonGround = 361,

            missileBaseNormal = 829,
            missileBaseDestroyed = 327,

            balambGardenW_static = 827,
            balambGardenE_static = 828,
            balambGardenE_mobile = 274,
            balambGardenW_mobile = 275,

            galbadiaGarden_static = 826,
            galbadiaGarden_mobile = 267,

            trabiaGardenE_state0 = 824,
            trabiaGardenW_state0 = 825,
            trabiaGardenE_state1 = 149,
            trabiaGardenW_state1 = 150,

            lunarCryCraterE_state0 = 830,
            lunarCryCraterW_state0 = 831,
            lunarCryCraterE_state1 = 214,
            lunarCryCraterW_state1 = 215,

            lunarCryCraterN_state1 = 246,
            lunarCryCraterS_state1 = 247,
            lunarCryCreaterN_state0 = 832,
            lunarCryCraterS_state0 = 833
        };

        /// <summary>
        /// This method changes zone i index to show segment that has to be drawn based on actual
        /// worldmap save state
        /// </summary>
        /// <param name="_i">index of current wmx segment</param>
        /// <param name="bfixCollision">
        /// use only with collision- due to inverted Balamb there's collision issue
        /// </param>
        /// <returns></returns>
        public static int SetInterchangeableZone(int _i)
        {
            //if(true) means unreversed world flags
            switch ((interZone)_i)
            {
                case interZone.prisonGround:
                    if (true)
                        return (int)interZone.prisonNormal;

                case interZone.missileBaseDestroyed:
                    if (true)
                        return (int)interZone.missileBaseNormal;

                case interZone.balambGardenE_mobile:
                    if (true)
                        return (int)interZone.balambGardenE_static - 1;

                case interZone.balambGardenW_mobile:
                    if (true)
                        return (int)interZone.balambGardenW_static + 1;

                case interZone.galbadiaGarden_mobile:
                    if (true)
                        return (int)interZone.galbadiaGarden_static;

                case interZone.trabiaGardenE_state1:
                    if (true)
                        return (int)interZone.trabiaGardenE_state0;

                case interZone.trabiaGardenW_state1:
                    if (true)
                        return (int)interZone.trabiaGardenW_state0;

                case interZone.lunarCryCraterE_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterE_state0;

                case interZone.lunarCryCraterW_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterW_state0;

                case interZone.lunarCryCraterN_state1:
                    if (true)
                        return (int)interZone.lunarCryCreaterN_state0;

                case interZone.lunarCryCraterS_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterS_state0;

                default:
                    return setEstharZones(_i);
            }
        }

        public static int setEstharZones(int i)
        {
            if (true) // esthar replace flag
            {
                if (Extended.In(i, 373, 380))
                    return i + 395;
                if (Extended.In(i, 405, 412))
                    return i + 371;
                if (Extended.In(i, 437, 444))
                    return i + 347;
                if (Extended.In(i, 469, 476))
                    return i + 323;
                if (Extended.In(i, 501, 508))
                    return i + 299;
                if (Extended.In(i, 533, 540))
                    return i + 275;
                if (Extended.In(i, 565, 572))
                    return i + 251;
            }
            return i; //compiler sake
        }

        /// <summary>
        /// This method returns the origX and origY coordinates for segment replacement for pre-parsing
        /// </summary>
        /// <param name="i">index of wm block</param>
        /// <returns></returns>
        public static int GetInterchangableSegmentReplacementIndex(int i)
        {
            if (i < 768)
                return i;

            if (i == (int)interZone.prisonNormal) //correct
                return (int)interZone.prisonGround;

            if (i == (int)interZone.missileBaseNormal)
                return (int)interZone.missileBaseDestroyed;

            if (i == (int)interZone.balambGardenE_static) //correct;
                return (int)interZone.balambGardenE_mobile + 1;
            if (i == (int)interZone.balambGardenW_static)
                return (int)interZone.balambGardenW_mobile - 1;

            if (i == (int)interZone.galbadiaGarden_static)
                return (int)interZone.galbadiaGarden_mobile;

            if (i == (int)interZone.trabiaGardenE_state0)
                return (int)interZone.trabiaGardenE_state1;
            if (i == (int)interZone.trabiaGardenW_state0)
                return (int)interZone.trabiaGardenW_state1;

            if (i == (int)interZone.lunarCryCraterE_state0)
                return (int)interZone.lunarCryCraterE_state1;
            if (i == (int)interZone.lunarCryCraterW_state0)
                return (int)interZone.lunarCryCraterW_state1;

            if (i == (int)interZone.lunarCryCreaterN_state0)
                return (int)interZone.lunarCryCraterN_state1;
            if (i == (int)interZone.lunarCryCraterS_state0)
                return (int)interZone.lunarCryCraterS_state1;

            if (Extended.In(i, 768, 775))
                return i - 395;
            if (Extended.In(i, 776, 783))
                return i - 371;
            if (Extended.In(i, 784, 791))
                return i - 347;
            if (Extended.In(i, 792, 799))
                return i - 323;
            if (Extended.In(i, 800, 807))
                return i - 299;
            if (Extended.In(i, 808, 815))
                return i - 275;
            if (Extended.In(i, 816, 823))
                return i - 251;

            return 0;
        }
    }
}
