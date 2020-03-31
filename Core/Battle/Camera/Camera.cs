using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenVIII.Fields.Scripts.Instructions;

namespace OpenVIII.Battle
{
    public partial class Camera
    {
        #region Fields

        public bool BMultiShotAnimation;

        public CameraStruct Cam;

        public uint LastCameraPointer;

        private readonly BattleCameraCollection _battleCameraCollection;

        private readonly uint _bsCameraPointer;

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

        private bool Done;

        #endregion Fields

        //private BattleCameraSettings _battleCameraSettings;

        #region Constructors

        public Camera() => _bsCameraPointer = GetCameraPointer();

        private Camera(BinaryReader br) : this()
        {
            br.BaseStream.Seek(_bsCameraPointer + 4, 0);
            //uint cCameraHeaderSector = br.ReadUInt16();
            //uint pCameraSetting = br.ReadUInt16();
            uint pCameraAnimationCollection = br.ReadUInt16();
            uint sCameraDataSize = br.ReadUInt16();

            //Camera settings parsing?
            //var battleCameraSettings = new BattleCameraSettings { unk = br.ReadBytes(24) };
            //end of camera settings parsing

            br.BaseStream.Seek(pCameraAnimationCollection + _bsCameraPointer, SeekOrigin.Begin);
            _battleCameraCollection = new BattleCameraCollection { cAnimCollectionCount = br.ReadUInt16() };
            var battleCameraSet = new BattleCameraSet[_battleCameraCollection.cAnimCollectionCount];
            _battleCameraCollection.battleCameraSet = battleCameraSet;
            for (var i = 0; i < _battleCameraCollection.cAnimCollectionCount; i++)
                battleCameraSet[i] = new BattleCameraSet { globalSetPointer = (uint)(br.BaseStream.Position + br.ReadUInt16() - i * 2 - 2) };
            _battleCameraCollection.pCameraEOF = br.ReadUInt16();

            for (var i = 0; i < _battleCameraCollection.cAnimCollectionCount; i++)
            {
                br.BaseStream.Seek(_battleCameraCollection.battleCameraSet[i].globalSetPointer, 0);
                _battleCameraCollection.battleCameraSet[i].animPointers = new uint[8];
                for (var n = 0; n < _battleCameraCollection.battleCameraSet[i].animPointers.Length; n++)
                    _battleCameraCollection.battleCameraSet[i].animPointers[n] = (uint)(br.BaseStream.Position + br.ReadUInt16() * 2 - n * 2);
            }
            Cam = Extended.ByteArrayToStructure<CameraStruct>(new byte[Marshal.SizeOf(typeof(CameraStruct))]); //what about this kind of trick to initialize struct with a lot amount of fixed sizes in arrays?

            ReadAnimationById(GetRandomCameraN(Memory.Encounters.Current), br);
            EndOffset = _bsCameraPointer + sCameraDataSize;
            //br.BaseStream.Seek(c.EndOffset, 0); //step out
        }

        #endregion Constructors

        #region Properties

        public Vector3 CamPosition { get; set; }
        public Vector3 CamTarget { get; set; }
        public uint EndOffset { get; set; }
        public Matrix ProjectionMatrix { get; set; }

        //public Matrix worldMatrix { get;  }
        public Matrix ViewMatrix { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Parses camera data into BattleCamera struct. Main purpose of this function is to
        /// actually read all the offsets and pointers to human readable form of struct. This
        /// function later calls ReadAnimation(n) where n is animation Id (i.e. 9 is camCollection=1
        /// and cameraAnim=0)
        /// </summary>
        public static Camera Read(BinaryReader br) => new Camera(br);

        public void ChangeAnimation(byte animId)
        {
            using (var br = Stage.Open())
                if (br != null)
                {
                    Cam.ResetTime();
                    ReadAnimationById(animId, br);
                }
        }

        public void Update()
        {
            if (!Done || !Cam.Done)
            {
                var tuple = Cam.UpdatePosition();
                if (tuple.CamPosition != tuple.CamTarget)
                {
                    (CamTarget, CamPosition, ViewMatrix, ProjectionMatrix) = tuple;
                    Debug.WriteLine((CamTarget, CamPosition, ViewMatrix, ProjectionMatrix));
                }
            }

            if (Cam.Done)
            {
                if (!BMultiShotAnimation || Cam.Time == 0)
                {
                    Done = true;
                    return;
                }
                using (var br = Stage.Open())
                    if (br != null)
                        ReadAnimation(LastCameraPointer - 2, br);
            }
            else
            {
                Cam.UpdateTime();
                Done = false;
            }
        }

        /// <summary>
        /// Returns tuple containing camera animation set pointer and camera animation in that set
        /// </summary>
        /// <param name="animId">6bit variable containing camera pointer</param>
        /// <returns>Tuple with CameraSetPointer, CameraSetPointer[CameraAnimationPointer]</returns>
        private static CameraSetAnimGRP GetCameraCollectionPointers(byte animId)
        {
            var enc = Memory.Encounters.Current;
            var pSet = enc.ResolveCameraSet(animId);
            var pAnim = enc.ResolveCameraAnimation(animId);
            return new CameraSetAnimGRP(pSet, pAnim);
        }

        /// <summary>
        /// Gets random camera from available from encounter- primary or secondary
        /// </summary>
        /// <param name="encounter">instance of current encounter</param>
        /// <returns>Either primary or alternative camera from encounter</returns>
        private static byte GetRandomCameraN(Encounter encounter)
        {
            var camToss = Memory.Random.Next(3) < 2 ? 0 : 1; //primary camera has 2/3 chance of being selected
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
        /// Gets vanilla engine camera pointers. A team that rewrote the game into PC just left
        /// PlayStation MIPS data inside files and therefore their code is to skip given hardcoded
        /// data which in fact are PS compiled instructions This data is naturally read by
        /// PlayStation in original console release.
        /// </summary>
        /// <returns>Camera pointer (data after PlayStation MIPS)</returns>
        private uint GetCameraPointer()
        {
            var scenario = Memory.Encounters.Current.Scenario;
            var _5d4 = _x5D4.Any(x => x == scenario);
            var _5d8 = _x5D8.Any(x => x == scenario);
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
        /// This method reads raw animation data in stage file or if br.BaseStream and br == null then br.BaseStream
        /// file and stores parsed data into battleCamera struct
        /// </summary>
        /// <param name="cameraAnimOffset">
        ///     if (br.BaseStream and br are null) is an offset in current battle stage file for camera animation.
        ///     If br.BaseStream and _br are provided it's the offset in this file
        /// </param>
        /// <param name="br">sub-component of ms</param>
        /// <param name="br.BaseStream">if null then stage file either this memory stream</param>
        /// <remarks See also = BS_Camera_ReadAnimation - 00503AC0></remarks>
        /// <returns></returns>
        private void ReadAnimation(uint cameraAnimOffset, BinaryReader br)
        {
            br.BaseStream.Seek(cameraAnimOffset, SeekOrigin.Begin);
            Cam.ReadAnimation(br);

            LastCameraPointer = (uint)(br.BaseStream.Position + 2);
            BMultiShotAnimation = br.ReadInt16() != -1;
        }

        /// <summary>
        /// This method resolves the correct camera pointer and runs ReadAnimation(uint,ms,br) method
        /// and returns the final pointer
        /// </summary>
        /// <param name="animId">
        ///     Animation Id as of binary mask (0bXXXXYYYY where XXXX= animationSet and YYYY=animationId)
        /// </param>
        /// <param name="br"></param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "CommentTypo")]
        private void ReadAnimationById(byte animId, BinaryReader br)
        {
            Cam.AnimationId = animId;
            var tpGetter = GetCameraCollectionPointers(animId);
            var battleCameraSetArray = _battleCameraCollection.battleCameraSet;
            if (battleCameraSetArray.Length > tpGetter.Set && battleCameraSetArray[tpGetter.Set].animPointers.Length > tpGetter.Anim)
            {
                var battleCameraSet = battleCameraSetArray[tpGetter.Set];
                var cameraAnimationGlobalPointer = battleCameraSet.animPointers[tpGetter.Anim];
                ReadAnimation(cameraAnimationGlobalPointer, br);
            }
            else
            {
                Memory.Log.WriteLine($"ReadAnimationById::{battleCameraSetArray.Length} < {tpGetter.Set}");

                if (battleCameraSetArray.Length > tpGetter.Set)
                    Memory.Log.WriteLine($" or \n{battleCameraSetArray[tpGetter.Set].animPointers.Length} < {tpGetter.Anim}");
            }
        }

        #endregion Methods
    }
}