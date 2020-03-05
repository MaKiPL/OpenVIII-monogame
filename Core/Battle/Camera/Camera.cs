using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Fields

        public bool bMultiShotAnimation;

        public CameraStruct cam;

        public uint lastCameraPointer;

        private readonly int[] _x5D4 = {4,5,9,12,13,14,15,21,22,23,24,26,
                29,32,33,34,35,36,39,40,50,53,55,61,62,63,64,65,66,67,68,69,70,
                71,72,73,75,78,82,83,85,86,87,88,89,90,91,94,96,97,98,99,100,105,
                106,121,122,123,124,125,126,127,135,138,141,144,145,148,149,150,
                151,158,160};

        private readonly int[] _x5D8 = {
                0,1,2,3,6,7,10,11,17,18,25,27,28,38,41,42,43,47,49,57,58,59,60,74,
                76,77,80,81,84,93,95,101,102,103,104,109,110,111,112,113,114,115,116,
                117,118,119,120,128,129,130,131,132,133,134,139,140,143,146,152,153,154,
                155,156,159,161,162};

        private readonly uint bs_cameraPointer;

        private BattleCameraCollection battleCameraCollection;

        private BattleCameraSettings battleCameraSettings;

        #endregion Fields

        #region Constructors

        public Camera() => bs_cameraPointer = GetCameraPointer();

        #endregion Constructors

        #region Properties

        public Vector3 camPosition { get; private set; }
        public Vector3 camTarget { get; private set; }
        public Matrix projectionMatrix { get; private set; }

        //public Matrix worldMatrix { get; private set; }
        public Matrix viewMatrix { get; private set; }

        public uint EndOffset { get; private set; }

        #endregion Properties

        #region Methods

        public void Update()
        {
            //const float V = 100f;
            //cam.startingTime = 64;
            float step = cam.CurrentTime.Ticks / (float)cam.TotalTime.Ticks;
            camTarget = Vector3.SmoothStep(cam.Camera_Lookat(0), cam.Camera_Lookat(1), step);
            camPosition = Vector3.SmoothStep(cam.Camera_World(0), cam.Camera_World(1), step);

            //            float camWorldX = MathHelper.Lerp(cam.Camera_World_X_s16[0] / V,
            //                cam.Camera_World_X_s16[1] / V, step) + 30;
            //            float camWorldY = MathHelper.Lerp(cam.Camera_World_Y_s16[0] / V,
            //                cam.Camera_World_Y_s16[1] / V, step) - 40;
            //            float camWorldZ = MathHelper.Lerp(cam.Camera_World_Z_s16[0] / V,
            //                cam.Camera_World_Z_s16[1] / V, step) + 0;

            //            float camTargetX = MathHelper.Lerp(cam.Camera_Lookat_X_s16[0] / V,
            //    cam.Camera_Lookat_X_s16[1] / V, step) + 30;
            //            float camTargetY = MathHelper.Lerp(cam.Camera_Lookat_Y_s16[0] / V,
            //cam.Camera_Lookat_Y_s16[1] / V, step) - 40;
            //            float camTargetZ = MathHelper.Lerp(cam.Camera_Lookat_Z_s16[0] / V,
            //cam.Camera_Lookat_Z_s16[1] / V, step) + 0;

            //camPosition = new Vector3(camWorldX, -camWorldY, -camWorldZ);
            //camTarget = new Vector3(camTargetX, -camTargetY, -camTargetZ);

            float fovDirector = MathHelper.SmoothStep(cam.startingFOV, cam.endingFOV, step);

            float fovD = (float)(2 * Math.Atan(240.0 / (2 * fovDirector)) * 57.29577951);

            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                   MathHelper.ToRadians(fovD),
                   Memory.graphics.GraphicsDevice.Viewport.AspectRatio,
    1f, 1000f);
            //worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
            //              Forward, Vector3.Up);

            //ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            //{
            //    Projection = projectionMatrix,
            //    View = viewMatrix,
            //    World = worldMatrix
            //};

            if (cam.CurrentTime >= cam.TotalTime)
            {
                if (bMultiShotAnimation && cam.time != 0)
                {
                    using (BinaryReader br = Stage.Open())
                        if (br != null)
                            ReadAnimation(lastCameraPointer - 2, br);
                }
            }
            else //cam.startingTime += Module_battle_debug.BATTLECAMERA_FRAMETIME;
                cam.UpdateTime();
        }

        /// <summary>
        /// Returns tuple containing camera animation set pointer and camera animation in that set
        /// </summary>
        /// <param name="animId">6bit variable containing camera pointer</param>
        /// <returns>Tuple with CameraSetPointer, CameraSetPointer[CameraAnimationPointer]</returns>
        private CameraSetAnimGRP GetCameraCollectionPointers(byte animId)
        {
            Battle.Encounter enc = Memory.Encounters.Current;
            int pSet = enc.ResolveCameraSet(animId);
            int pAnim = enc.ResolveCameraAnimation(animId);
            return new CameraSetAnimGRP(pSet, pAnim);
        }

        /// <summary>
        /// Gets vanilla engine camera pointers. A team that rewrote the game into PC just left
        /// PlayStation MIPS data inside files and therefore their code is to skip given hardcoded
        /// data which in fact are PS compiled instructions This data is naturally read by
        /// PlayStation in original console release.
        /// </summary>
        /// <returns>Camera pointer (data after PlayStation MIPS)</returns>
        private uint GetCameraPointer()
        {
            byte scenario = Memory.Encounters.Current.Scenario;
            bool _5d4 = _x5D4.Any(x => x == scenario);
            bool _5d8 = _x5D8.Any(x => x == scenario);
            if (_5d4) return 0x5D4;
            if (_5d8) return 0x5D8;
            switch (scenario)
            {
                case 8:
                case 48:
                case 79:
                    return 0x618;

                case 16:
                    return 0x628;

                case 19:
                    return 0x644;

                case 20:
                    return 0x61c;

                case 30:
                case 31:
                    return 0x934;

                case 37:
                    return 0xcc0;

                case 44:
                case 45:
                case 46:
                    return 0x9A4;

                case 51:
                case 52:
                case 107:
                case 108:
                    return 0x600;

                case 54:
                case 56:
                    return 0x620;

                case 92:
                    return 0x83c;

                case 136:
                    return 0x5fc;

                case 137:
                    return 0xFDC;

                case 142:
                    return 0x183C;

                case 147:
                    return 0x10f0;

                case 157:
                    return 0x638;
            }
            throw new Exception("0xFFF, unknown pointer!");
        }

        /// <summary>
        /// Gets random camera from available from encounter- primary or secondary
        /// </summary>
        /// <param name="encounter">instance of current encounter</param>
        /// <returns>Either primary or alternative camera from encounter</returns>
        private byte GetRandomCameraN(Battle.Encounter encounter)
        {
            int camToss = Memory.Random.Next(3) < 2 ? 0 : 1; //primary camera has 2/3 chance of beign selected
            switch (camToss)
            {
                case 0:
                    return encounter.PrimaryCamera;

                case 1:
                    return encounter.AlternativeCamera;

                default:
                    goto case 0;
            }
        }

        /// <summary>
        /// This method reads raw animation data in stage file or if br.BaseStream and br == null then br.BaseStream
        /// file and stores parsed data into battleCamera struct
        /// </summary>
        /// <param name="cameraAnimOffset">
        /// if (br.BaseStream and br are null) is an offset in current battle stage file for camera animation.
        /// If br.BaseStream and _br are provided it's the offset in this file
        /// </param>
        /// <param name="br.BaseStream">if null then stage file either this memory stream</param>
        /// <param name="br">sub-component of ms</param>
        /// <remarks See also = BS_Camera_ReadAnimation - 00503AC0></remarks>
        /// <returns></returns>
        private uint ReadAnimation(uint cameraAnimOffset, BinaryReader br)
        {
            short local2C;
            byte keyframecount = 0;
            ushort totalframecount = 0;
            short local1C;
            short local18;
            short local14;
            short local10;

            br.BaseStream.Seek(cameraAnimOffset, SeekOrigin.Begin);
            cam.control_word = br.ReadUInt16();
            if (cam.control_word == 0xFFFF)
                return 0; //return NULL

            ushort current_position = br.ReadUInt16(); //getter for *current_position
            br.BaseStream.Seek(-2, SeekOrigin.Current); //roll back one WORD because no increment

            switch ((cam.control_word >> 6) & 3)
            {
                case 1:
                    cam.startingFOV = 0x200;
                    cam.endingFOV = 0x200;
                    break;

                case 2:
                    cam.startingFOV = current_position;
                    cam.endingFOV = current_position;
                    br.ReadUInt16(); //current_position++
                    break;

                case 3:
                    cam.startingFOV = current_position;
                    br.BaseStream.Seek(2, SeekOrigin.Current); //skipping WORD, because we already rolled back one WORD above this switch
                    current_position = br.ReadUInt16();
                    cam.endingFOV = current_position;
                    break;
            }
            switch ((cam.control_word >> 8) & 3)
            {
                case 0: //TODO!!
                    cam.startingCameraRoll = 00000000; //TODO, what's ff8vars.unkword1D977A2?
                    cam.endingCameraRoll = 00000000; //same as above; cam->unkword00A = ff8vars.unkword1D977A2;
                    break;

                case 1:
                    cam.startingCameraRoll = 0;
                    cam.endingCameraRoll = 0;
                    break;

                case 2:
                    current_position = br.ReadUInt16(); //* + current_position++;
                    cam.startingCameraRoll = current_position;
                    cam.endingCameraRoll = current_position;
                    break;

                case 3:
                    current_position = br.ReadUInt16(); //* + current_position++;
                    cam.startingCameraRoll = current_position;
                    current_position = br.ReadUInt16(); //* + current_position++;
                    cam.endingCameraRoll = current_position;
                    break;
            }

            switch (cam.control_word & 1)
            {
                case 0:
                    if (current_position >= 0)
                    {
                        while (true) //I'm setting this to true and breaking in code as this works on peeking on next variable via pointer and that's not possible here without unsafe block
                        {
                            cam.startFramesOffsets[keyframecount] = totalframecount; //looks like this is the camera index
                            current_position = br.ReadUInt16();
                            if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                break;
                            totalframecount += (ushort)(current_position * 16); //here is increment of short*, but I already did that above
                            cam.is_FrameDurationsShot[keyframecount] = (byte)(current_position = br.ReadUInt16()); //cam->unkbyte124[keyframecount] = *current_position++; - looks like we are wasting one byte due to integer sizes
                            cam.Camera_World_X_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            cam.Camera_World_Y_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            cam.Camera_World_Z_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            cam.is_FrameEndingShots[keyframecount] = (byte)(current_position = br.ReadUInt16()); //m->unkbyte204[keyframecount] = *current_position++;
                            cam.Camera_Lookat_X_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            cam.Camera_Lookat_Y_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            cam.Camera_Lookat_Z_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            keyframecount++;
                        }

                        if (keyframecount > 2)
                        {
                            //ff8funcs.Sub50D010(cam->unkword024, cam->unkword064, cam->unkword0A4, cam->unkword0E4, keyframecount, cam->unkbyte224, cam->unkbyte2A4, cam->unkbyte324);
                            //ff8funcs.Sub50D010(cam->unkword024, cam->unkword144, cam->unkword184, cam->unkword1C4, keyframecount, cam->unkbyte3A4, cam->unkbyte424, cam->unkbyte4A4);
                        }
                    }
                    break;

                case 1:
                    {
                        goto case 0;
                        if (current_position >= 0)
                        {
                            local14 = (short)(br.BaseStream.Position + 5 * 2); //current_position + 5; but current_position is WORD, so multiply by two
                            local10 = (short)(br.BaseStream.Position + 6 * 2);
                            local2C = (short)(br.BaseStream.Position + 7 * 2);
                            local18 = (short)(br.BaseStream.Position + 1 * 2);
                            local1C = (short)(br.BaseStream.Position + 2 * 2);
                            while (true)
                            {
                                cam.startFramesOffsets[keyframecount] = totalframecount;
                                current_position = br.ReadUInt16();
                                if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                    break;
                                totalframecount += (ushort)(current_position * 16);
                                //ff8funcs.Sub503AE0(++local18, ++local1C, ++ebx, *(BYTE*)current_position, &cam->unkword064[keyframecount], &cam->unkword0A4[keyframecount], &cam->unkword0E4[keyframecount]);
                                //ff8funcs.Sub503AE0(++local14, ++local10, ++local2C, *(BYTE*)(current_position + 4), &cam->unkword144[keyframecount], &cam->unkword184[keyframecount], &cam->unkword1C4[keyframecount]);
                                cam.is_FrameEndingShots[keyframecount] = 0xFB;
                                cam.is_FrameDurationsShot[keyframecount] = 0xFB;
                                local1C += 8;
                                local18 += 8;
                                current_position += 8;
                                local2C += 8;
                                //ebx += 8;
                                local10 += 8;
                                local14 += 8;
                                keyframecount++;
                            }
                        }
                        break;
                    }
            }

            if ((cam.control_word & 0x3E) == 0x1E)
            {
                //ff8funcs.Sub503300();
            }
            cam.keyframeCount = keyframecount;
            cam.time = totalframecount;
            //cam.startingTime = 0;
            cam.CurrentTime = TimeSpan.Zero;
            lastCameraPointer = (uint)(br.BaseStream.Position + 2);
            bMultiShotAnimation = br.ReadInt16() != -1;
            return (uint)(br.BaseStream.Position);
        }

        public void ChangeAnimation(byte animId)
        {
            using (BinaryReader br = Stage.Open())
                if (br != null)
                {
                    cam.ResetTime();
                    ReadAnimationById(animId, br);
                }
        }

        /// <summary>
        /// This method resolves the correct camera pointer and runs ReadAnimation(uint,ms,br) method
        /// and returns the final pointer
        /// </summary>
        /// <param name="animId">
        /// Animation Id as of binary mask (0bXXXXYYYY where XXXX= animationSet and YYYY=animationId)
        /// </param>
        /// <returns></returns>
        private uint ReadAnimationById(byte animId, BinaryReader br)
        {
            cam.animationId = animId;
            CameraSetAnimGRP tpGetter = GetCameraCollectionPointers(animId);
            BattleCameraSet[] battleCameraSetArray = battleCameraCollection.battleCameraSet;
            if (battleCameraSetArray.Length > tpGetter.Set && battleCameraSetArray[tpGetter.Set].animPointers.Length > tpGetter.Anim)
            {
                BattleCameraSet battleCameraSet = battleCameraSetArray[tpGetter.Set];
                uint cameraAnimationGlobalPointer = battleCameraSet.animPointers[tpGetter.Anim];
                return ReadAnimation(cameraAnimationGlobalPointer, br);
            }
            else
            {
                Memory.Log.WriteLine($"ReadAnimationById::{battleCameraSetArray.Length} < {tpGetter.Set}");

                if (battleCameraSetArray.Length > tpGetter.Set)
                    Memory.Log.WriteLine($" or \n{battleCameraSetArray[tpGetter.Set].animPointers.Length} < {tpGetter.Anim}");
            }
            return 0;
        }


        /// <summary>
        /// Parses camera data into BattleCamera struct. Main purpouse of this function is to
        /// actually read all the offsets and pointers to human readable form of struct. This
        /// function later calls ReadAnimation(n) where n is animation Id (i.e. 9 is camCollection=1
        /// and cameraAnim=0)
        /// </summary>
        public static Camera Read(BinaryReader br)
        {
            Camera c = new Camera();
            br.BaseStream.Seek(c.bs_cameraPointer, 0);
            uint cCameraHeaderSector = br.ReadUInt16();
            uint pCameraSetting = br.ReadUInt16();
            uint pCameraAnimationCollection = br.ReadUInt16();
            uint sCameraDataSize = br.ReadUInt16();

            //Camera settings parsing?
            BattleCameraSettings bcs = new BattleCameraSettings() { unk = br.ReadBytes(24) };
            //end of camera settings parsing

            br.BaseStream.Seek(pCameraAnimationCollection + c.bs_cameraPointer, SeekOrigin.Begin);
            BattleCameraCollection bcc = new BattleCameraCollection { cAnimCollectionCount = br.ReadUInt16() };
            BattleCameraSet[] bcset = new BattleCameraSet[bcc.cAnimCollectionCount];
            bcc.battleCameraSet = bcset;
            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
                bcset[i] = new BattleCameraSet() { globalSetPointer = (uint)(br.BaseStream.Position + br.ReadUInt16() - i * 2 - 2) };
            bcc.pCameraEOF = br.ReadUInt16();

            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
            {
                br.BaseStream.Seek(bcc.battleCameraSet[i].globalSetPointer, 0);
                bcc.battleCameraSet[i].animPointers = new uint[8];
                for (int n = 0; n < bcc.battleCameraSet[i].animPointers.Length; n++)
                    bcc.battleCameraSet[i].animPointers[n] = (uint)(br.BaseStream.Position + br.ReadUInt16() * 2 - n * 2);
            }
            CameraStruct cam = Extended.ByteArrayToStructure<CameraStruct>(new byte[Marshal.SizeOf(typeof(CameraStruct))]); //what about this kind of trick to initialize struct with a lot amount of fixed sizes in arrays?
            c.battleCameraCollection = bcc;
            c.battleCameraSettings = bcs;
            c.cam = cam;

            c.ReadAnimationById(c.GetRandomCameraN(Memory.Encounters.Current), br);
            c.EndOffset = c.bs_cameraPointer + sCameraDataSize;
            //br.BaseStream.Seek(c.EndOffset, 0); //step out
            return c;
        }

        #endregion Methods
    }
}