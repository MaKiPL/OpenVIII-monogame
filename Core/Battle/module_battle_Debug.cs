using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenVIII
{
    public static class Module_battle_debug
    {
        private static DeadTime DeadTime;
        private static Battle.RegularPyramid RegularPyramid;
        private static uint bs_cameraPointer;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees = 90;

        private static Vector3 camPosition, camTarget;
        private static TIM2 textureInterface;
        private static TextureHandler[] textures;

        //private static List<EnemyInstanceInformation> EnemyInstances;
        private static List<CharacterInstanceInformation> CharacterInstances;

        //skyRotating floats are hardcoded
        private static readonly ushort[] skyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };

        private static float localRotator = 0.0f; //a rotator is a float that holds current axis rotation for sky. May be malformed by skyRotators or TimeCompression magic

        public static BasicEffect effect;
        public static AlphaTestEffect ate;

        private static string battlename = "a0stg000.x";
        private static byte[] stageBuffer;

        private static int battleModule = 0;

        private static FPS_Camera fps_camera;

        //This should be enum btw
        private const int BATTLEMODULE_INIT = 0; //basic init stuff; renderer; core

        private const int BATTLEMODULE_READDATA = 1; //parses battle stage and all monsters
        private const int BATTLEMODULE_DRAWGEOMETRY = 2; //draw geometry also supports updateCamera
        private const int BATTLEMODULE_ACTIVE = 3;
        private static readonly TimeSpan FPS = TimeSpan.FromMilliseconds(1000.0d / 15d); //Natively the game we are rewritting works in 15 FPS per second

        /// <summary>
        /// controls the amount of battlecamera.time incrementation- lower value means longer camera animation
        /// </summary>
        private const int BATTLECAMERA_FRAMETIME = 3;
        public const int Yoffset = -10;

        /// <summary>
        /// This is helper struct that works along with VertexPosition to provide Clut, texture page
        /// and bool to decide if it's quad or triangle
        /// </summary>
        private struct Stage_GeometryInfoSupplier
        {
            public bool bQuad;
            public byte clut;
            public byte texPage;
        }

        public class EnemyInstanceInformation
        {
            public Debug_battleDat Data;

            /// <summary>
            /// bit position of the enemy in encounter data. Use to pair the information with
            /// encounter data
            /// </summary>
            public byte index;

            public bool bIsHidden;
            public bool bIsActive;
            public bool bIsUntargetable;
            public AnimationSystem animationSystem;
        }

        /// <summary>
        /// CharacterInstanceInformation should only be used for battle-exclusive data. Manipulating
        /// HP, GFs, junctions and other character-specific things should happen outside battle,
        /// because such information about characters is shared between almost all modules. This
        /// field contains information about the current status of battle rendering like animation
        /// frames/ rendering flags/ effects attached
        /// </summary>
        public class CharacterInstanceInformation
        {
            public CharacterData Data;
            public int characterId; //0 is Whatever guy
            public Characters VisibleCharacter => (Characters)Data.character.GetId;
            public bool bIsHidden; //GF sequences, magic...
            public AnimationSystem animationSystem;

            public void SetAnimationID(int id)
            {
                if (animationSystem.AnimationId != id &&
                    id < Data.character.animHeader.animations.Length &&
                    id < Data.weapon.animHeader.animations.Length &&
                    id >= 0)
                {
                    animationSystem.AnimationId = id;
                }
            }
        }

        private struct Triangle
        {
            public ushort A;
            public ushort B;
            public ushort C;
            public byte U1;
            public byte V1;
            public byte U2;
            public byte V2;
            public byte clut;
            public byte U3;
            public byte V3;
            public byte TexturePage;
            public byte bHide;
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte GPU;
        }

        private struct Quad
        {
            public ushort A;
            public ushort B;
            public ushort C;
            public ushort D;
            public byte U1;
            public byte V1;
            public byte clut;
            public byte U2;
            public byte V2;
            public byte TexturePage;
            public byte bHide;
            public byte U3;
            public byte V3;
            public byte U4;
            public byte V4;
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte GPU;
        }

        private struct MainGeometrySection
        {
            public uint Group1Pointer;
            public uint Group2Pointer;
            public uint Group3Pointer;
            public uint Group4Pointer;
            public uint TextureUNUSEDPointer;
            public uint TexturePointer;
            public uint EOF;
        }

        private struct ObjectsGroup
        {
            public uint numberOfSections;
            public uint settings1Pointer;
            public uint objectListPointer;
            public uint settings2Pointer;
            public uint relativeEOF;
        }

        private struct Vertex
        {
            public short X;
            public short Y;
            public short Z;
        }

        private struct Model
        {
            public Vertex[] vertices;
            public Triangle[] triangles;
            public Quad[] quads;
        }

        private struct ModelGroup
        {
            public Model[] models;
        }

        private static ModelGroup[] modelGroups;

        private static Debug_battleDat[] monstersData;

        public struct CharacterData
        {
            public Debug_battleDat character, weapon;
        };

        private static MemoryStream ms;
        private static BinaryReader br;

        private static byte GetTexturePage(byte texturepage) => (byte)(texturepage & 0x0F);

        private static byte GetClutId(ushort clut)
        {
            ushort bb = Extended.UshortLittleEndian(clut);
            return (byte)(((bb >> 14) & 0x03) | (bb << 2) & 0x0C);
        }

        public static void ResetState() => battleModule = BATTLEMODULE_INIT;

        public static bool PauseATB = false;
        private static sbyte? partypos = null;

        public static void Update()
        {
            DeadTime?.Update();
            if (CharacterInstances != null)
                foreach (CharacterInstanceInformation cii in CharacterInstances)
                {
                    Saves.CharacterData c = Memory.State?[cii.VisibleCharacter];
                    c.Update(); //updates ATB for Character.
                    if (c != null && cii.animationSystem.AnimationId >= 0 && cii.animationSystem.AnimationId <= 2)
                    {
                        // this would probably interfeer with other animations. I am hoping the limits above will keep it good.
                        if (c.IsDead)
                            cii.SetAnimationID((int)AnimID.Dead);
                        else if (c.IsCritical)
                            cii.SetAnimationID((int)AnimID.Critical);
                    }
                    //if (Menu.BattleMenus.Victory_Menu.Enabled)
                    //    cii.SetAnimationID(20);
                    //cii.SetAnimationID(31);
                }
            if (Enemy.Party != null)
                foreach (Enemy e in Enemy.Party)
                    e.Update(); //updates ATB for enemy.
            bool ret = false;
            switch (battleModule)
            {
                case BATTLEMODULE_INIT:
                    InitBattle();
                    break;

                case BATTLEMODULE_READDATA:
                    ReadData();
                    Menu.BattleMenus.Refresh();
                    Menu.FadeIn();
                    break;

                case BATTLEMODULE_DRAWGEOMETRY:
                    Menu.BattleMenus.Update();
                    sbyte? partypos = Menu.BattleMenus.PartyPos;
                    if (partypos != Module_battle_debug.partypos)
                    {
                        if (partypos == null)
                        {
                            RegularPyramid.FadeOut();
                        }
                        else
                        {
                            RegularPyramid.Set(GetPos(partypos ?? 0) + PyramidOffset);
                        }
                        Module_battle_debug.partypos = partypos;
                    }
                    if (bUseFPSCamera)
                        viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                    else
                    {
                        UpdateCamera();
                        ret = Menu.BattleMenus.Inputs();
                    }
                    break;
            }
            if (!ret) Inputs();
            RegularPyramid.Update();
            UpdateFrames();
        }

        public static void Inputs()
        {
#if DEBUG

            if (Input2.Button(Keys.D0))
                bUseFPSCamera = !bUseFPSCamera;
            if (Input2.Button(Keys.D1))
                if ((DEBUGframe & 0b1111) >= 7)
                {
                    DEBUGframe += 0b00010000;
                    DEBUGframe -= 7;
                }
                else DEBUGframe += 1;
            if (Input2.Button(Keys.D2))
                if ((DEBUGframe & 0b1111) == 0)
                {
                    DEBUGframe -= 0b00010000;
                    DEBUGframe += 7;
                }
                else DEBUGframe--;
            if (Input2.Button(Keys.D3))
                battleModule = BATTLEMODULE_INIT;
            if (Input2.Button(Keys.D4))
            {
                battleModule = BATTLEMODULE_INIT;
                Memory.battle_encounter++;
            }
            if (Input2.Button(Keys.D5))
            {
                AddAnimationToQueue(Debug_battleDat.EntityType.Monster, 0, 3);
                AddAnimationToQueue(Debug_battleDat.EntityType.Monster, 0, 0);
            }
            if (Input2.Button(Keys.F12))
            {
                if (SID < 255)
                    SID++;
                else SID = 0;
            }

            if (Input2.Button(Keys.F11))
            {
                if (SID <= 0)
                    SID = 255;
                else SID--;
            }
            if (Input2.Button(Keys.F10))
            {
                AddSequenceToAllQueues(SID);
            }
            if (Input2.Button(Keys.F9))
            {
                AddSequenceToAllQueues(new Debug_battleDat.AnimationSequence
                {
                    AnimationQueue = new List<byte> {
                    //0x2,
                    //0x5,
                    //0xf,
                    //0x10,
                    //0xb,
                    //0x3,
                    //0x6,
                    0xe,
                    //0x1,
                    0xf,
                    0x0
                }
                });
            }

            if (Input2.Button(Keys.F8))
            {
                StopAnimations();
            }
            if (Input2.Button(Keys.F7))
            {
                StartAnimations();
            }
#endif
        }

        private static void AddSequenceToAllQueues(Debug_battleDat.AnimationSequence section5)
        {
            for (int i = 0; i < Enemy.Party.Count; i++)
            {
                AddSequenceToQueue(Debug_battleDat.EntityType.Monster, i, section5);
            }
            for (int i = 0; i < CharacterInstances.Count; i++)
            {
                AddSequenceToQueue(Debug_battleDat.EntityType.Character, i, section5);
            }
        }

        private static void AddSequenceToAllQueues(byte sid)
        {
            Debug_battleDat.AnimationSequence section5;
            for (int i = 0; i < Enemy.Party.Count; i++)
            {
                if (Enemy.Party[i].EII.Data.Sequences.Count > sid)
                {
                    section5 = Enemy.Party[i].EII.Data.Sequences.FirstOrDefault(x => x.id == sid);
                    if (section5.AnimationQueue != null)
                        AddSequenceToQueue(Debug_battleDat.EntityType.Monster, i, section5);
                    //AddAnimationToQueue(Debug_battleDat.EntityType.Monster, i, 0);
                }
            }
            for (int i = 0; i < CharacterInstances.Count; i++)
            {
                Debug_battleDat weapon = CharacterInstances[i].Data.weapon;
                Debug_battleDat character = CharacterInstances[i].Data.character;
                List<Debug_battleDat.AnimationSequence> sequences;
                if ((weapon?.Sequences.Count ?? 0) == 0)
                {
                    sequences = character.Sequences;
                }
                else sequences = weapon.Sequences;
                if (sequences.Count > sid)
                {
                    section5 = sequences.FirstOrDefault(x => x.id == sid);
                    if (section5.AnimationQueue != null)
                        AddSequenceToQueue(Debug_battleDat.EntityType.Character, i, section5);
                    //AddAnimationToQueue(Debug_battleDat.EntityType.Character, i, 0);
                }
            }
        }

        private static byte SID = 0;

        public static void Draw()
        {
            switch (battleModule)
            {
                case BATTLEMODULE_DRAWGEOMETRY:
                    DrawGeometry();
                    DrawMonsters();
                    DrawCharactersWeapons();
                    RegularPyramid.Draw(worldMatrix, viewMatrix, projectionMatrix);
                    if (!bUseFPSCamera)
                        Menu.BattleMenus.Draw();
                    break;
            }
        }

        private static void UpdateCamera()
        {
            //const float V = 100f;
            //battleCamera.cam.startingTime = 64;
            float step = battleCamera.cam.CurrentTime.Ticks / (float)battleCamera.cam.TotalTime.Ticks;
            camTarget = Vector3.SmoothStep(battleCamera.cam.Camera_Lookat(0),battleCamera.cam.Camera_Lookat(1),step);
            camPosition = Vector3.SmoothStep(battleCamera.cam.Camera_World(0), battleCamera.cam.Camera_World(1), step);


            //            float camWorldX = MathHelper.Lerp(battleCamera.cam.Camera_World_X_s16[0] / V,
            //                battleCamera.cam.Camera_World_X_s16[1] / V, step) + 30;
            //            float camWorldY = MathHelper.Lerp(battleCamera.cam.Camera_World_Y_s16[0] / V,
            //                battleCamera.cam.Camera_World_Y_s16[1] / V, step) - 40;
            //            float camWorldZ = MathHelper.Lerp(battleCamera.cam.Camera_World_Z_s16[0] / V,
            //                battleCamera.cam.Camera_World_Z_s16[1] / V, step) + 0;

            //            float camTargetX = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_X_s16[0] / V,
            //    battleCamera.cam.Camera_Lookat_X_s16[1] / V, step) + 30;
            //            float camTargetY = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_Y_s16[0] / V,
            //battleCamera.cam.Camera_Lookat_Y_s16[1] / V, step) - 40;
            //            float camTargetZ = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_Z_s16[0] / V,
            //battleCamera.cam.Camera_Lookat_Z_s16[1] / V, step) + 0;

            //camPosition = new Vector3(camWorldX, -camWorldY, -camWorldZ);
            //camTarget = new Vector3(camTargetX, -camTargetY, -camTargetZ);

            float fovDirector = MathHelper.SmoothStep(battleCamera.cam.startingFOV, battleCamera.cam.endingFOV, step);

            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                   MathHelper.ToRadians(fovDirector / 8),
                   Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
    1f, 1000f);

            //ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            //{
            //    Projection = projectionMatrix,
            //    View = viewMatrix,
            //    World = worldMatrix
            //};

            if (battleCamera.cam.CurrentTime >= battleCamera.cam.TotalTime)
            {
                if (battleCamera.bMultiShotAnimation && battleCamera.cam.time != 0)
                {
                    ReopenStageStreams();
                    ReadAnimation(battleCamera.lastCameraPointer - 2);
                    CloseStageStreams();
                }
            }
            else //battleCamera.cam.startingTime += Module_battle_debug.BATTLECAMERA_FRAMETIME;
                battleCamera.cam.UpdateTime();
        }

        private static void ReopenStageStreams()
        {
            ms = new MemoryStream(stageBuffer);
            br = new BinaryReader(ms);
        }

        private static void CloseStageStreams()
        {
            if (br != null)
                br.Close();
            if (ms != null) { ms.Close(); ms.Dispose(); }
        }

        /// <summary>
        /// Method to render characters and weapons for them
        /// </summary>
        private static void DrawCharactersWeapons()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;
            effect.TextureEnabled = true;
            //CHARACTER
            if (CharacterInstances == null)
                return;

            for (int n = 0; n < CharacterInstances.Count; n++)
            {
                CheckAnimationFrame(Debug_battleDat.EntityType.Character, n);
                Vector3 charaPosition = GetCharPos(n);
                UpdatePos(CharacterInstances[n].Data.character, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].animationSystem, ref charaPosition);
                DrawShadow(charaPosition, ate, .5f);

                //WEAPON
                if (CharacterInstances[n].Data.weapon != null)
                {
                    CheckAnimationFrame(Debug_battleDat.EntityType.Weapon, n);
                    UpdatePos(CharacterInstances[n].Data.weapon, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].animationSystem, ref charaPosition);
                }
            }
        }

        private static Vector3 GetCharPos(int _n) => new Vector3(-10 + _n * 10, Yoffset, -30);

        private static void UpdatePos(Debug_battleDat battledat, double step, ref AnimationSystem animationSystem, ref Vector3 position, Quaternion? _rotation = null)
        {
            for (int i = 0; i < battledat.geometry.cObjects; i++)
            {
                Quaternion rotation = _rotation ?? Quaternion.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);
                Debug_battleDat.VertexPositionTexturePointersGRP vptpg = battledat.GetVertexPositions(
                    i,
                    ref position,
                    rotation,
                    ref animationSystem,
                    step); //DEBUG
                if (vptpg.IsNotSet())
                    return;
                for (int k = 0; k < vptpg.VPT.Length / 3; k++)
                {
                    ate.Texture = (Texture2D)battledat.textures.textures[vptpg.TexturePointers[k]];
                    foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                        vertexData: vptpg.VPT, vertexOffset: k * 3, primitiveCount: 1);
                    }
                }
            }
            return;
        }

        private static double CharacterInstanceGenerateStep(int n) => GenerateStep(CharacterInstanceAnimationStopped(n));

        private static bool CharacterInstanceAnimationStopped(int n) =>
            CharacterInstances[n].animationSystem.AnimationStopped ||
            ((Memory.State?[(Characters)CharacterInstances[n].characterId]?.IsPetrify ?? false) &&
            CharacterInstances[n].animationSystem.StopAnimation());

        /// <summary>
        /// This function is responsible for deleting the queue of animation if passed correctly
        /// </summary>
        /// <param name="type"></param>
        /// <param name="n"></param>
        private static void CheckAnimationFrame(Debug_battleDat.EntityType type, int n)
        {
            Debug_battleDat.Animation animationSystem;
            switch (type)
            {
                case Debug_battleDat.EntityType.Monster:
                    animationSystem = Enemy.Party[n].EII.Data.animHeader.animations[Enemy.Party[n].EII.animationSystem.AnimationId];
                    if (Enemy.Party[n].EII.animationSystem.AnimationFrame >= animationSystem.cFrames)
                    {
                        EnemyInstanceInformation InstanceInformationProvider = Enemy.Party[n].EII;
                        if (Enemy.Party[n].EII.animationSystem.AnimationQueue.TryDequeue(out int animid) &&
                           animid < InstanceInformationProvider.Data.animHeader.animations.Length &&
                           animid >= 0
                            )
                        {
                            InstanceInformationProvider.animationSystem.AnimationId = animid;
                        }
                        Enemy.Party[n].EII = InstanceInformationProvider;
                    }
                    return;

                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    animationSystem = CharacterInstances[n].Data.character.animHeader.animations[CharacterInstances[n].animationSystem.AnimationId];
                    if (CharacterInstances[n].animationSystem.AnimationFrame >= animationSystem.cFrames)
                    {
                        CharacterInstanceInformation InstanceInformationProvider = CharacterInstances[n];
                        if (CharacterInstances[n].animationSystem.AnimationQueue.TryDequeue(out int animid) &&
                           (animid < InstanceInformationProvider.Data.character.animHeader.animations.Length ||
                           animid < (InstanceInformationProvider.Data.weapon?.animHeader.animations.Length ?? 0)) &&
                           animid >= 0)
                        {
                            InstanceInformationProvider.animationSystem.AnimationId = animid;
                        }
                        CharacterInstances[n] = InstanceInformationProvider;
                    }
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// Animation system. Decided to go for struct, so I can attach it to instance and manipulate
        /// easily grouped. It's also open for modifications
        /// </summary>
        public struct AnimationSystem
        {
            public int AnimationId
            {
                get => _animationId; set
                {
                    _lastAnimationId = _animationId;
                    _animationId = value;
                    AnimationFrame = 0;
                }
            }

            public int AnimationFrame
            {
                get => _animationFrame; set
                {
                    _lastAnimationFrame = _animationFrame;
                    _animationFrame = value;
                    if (_animationFrame > 0 && _lastAnimationId != _animationId)
                        _lastAnimationId = _animationId;
                }
            }

            public int NextFrame() => ++AnimationFrame;

            public int LastAnimationId { get => _lastAnimationId; private set => _lastAnimationId = value; }
            public int LastAnimationFrame { get => _lastAnimationFrame; private set => _lastAnimationFrame = value; }

            public bool AnimationStopped => bAnimationStopped;

            public bool StopAnimation()
            {
                LastAnimationFrame = AnimationFrame;
                AnimationId = AnimationId;
                return bAnimationStopped = true;
            }

            public bool StartAnimation() => bAnimationStopped = false;

            private bool bAnimationStopped; //pertification placeholder?
            public ConcurrentQueue<int> AnimationQueue;
            private int _lastAnimationFrame;
            private int _animationFrame;
            private int _lastAnimationId;
            private int _animationId;
        }

        public static int DEBUGframe = 0;

        private static void DrawMonsters()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;
            effect.TextureEnabled = true;

            if (Enemy.Party == null)
                return;
            for (int n = 0; n < Enemy.Party.Count; n++)
            {
                if (Enemy.Party[n].EII.Data.GetId == 127)
                {
                    //TODO;
                    continue;
                }

                CheckAnimationFrame(Debug_battleDat.EntityType.Monster, n);
                Vector3 enemyPosition = GetEnemyPos(n);
                enemyPosition.Y += Yoffset;
                UpdatePos(Enemy.Party[n].EII.Data, GenerateStep(EnemyInstanceAnimationStopped(n)), ref Enemy.Party[n].EII.animationSystem, ref enemyPosition, Quaternion.CreateFromYawPitchRoll(0f, 0f, 0f));
                DrawShadow(enemyPosition, ate, Enemy.Party[n].EII.Data.skeleton.GetScale.X / 5);
            }
        }

        private static Vector3 GetPos(int n) => n >= 0 ? GetCharPos(n)*new Vector3(1,0,1)+new Vector3(0,CharacterInstances[n].Data.character.Highpoint,0) : 
            GetEnemyPos(-n - 1) * new Vector3(1, 0, 1) + new Vector3(0,Enemy.Party[-n - 1].EII.Data.Highpoint,0);

        private static Vector3 GetEnemyPos(int n) =>
                        Memory.encounters[Memory.battle_encounter].enemyCoordinates.GetEnemyCoordinateByIndex(Enemy.Party[n].EII.index).GetVector();

        private static bool EnemyInstanceAnimationStopped(int n) =>
            Enemy.Party[n].EII.animationSystem.AnimationStopped ||
            (Enemy.Party[n].IsPetrify &&
            Enemy.Party[n].EII.animationSystem.StopAnimation());

        private static void StopAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.animationSystem.StopAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.animationSystem.StopAnimation();
        }

        private static void StartAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.animationSystem.StartAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.animationSystem.StartAnimation();
        }

        private static double GenerateStep(bool AnimationStopped)
        {
            if (AnimationStopped)
                return 1d;
            return (double)FrameTime.Ticks / FPS.Ticks;
        }

        /// <summary>
        /// [BROKEN] See issue #46
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="ate"></param>
        /// <param name="scale"></param>
        private static void DrawShadow(Vector3 enemyPosition, AlphaTestEffect ate, float scale)
        {
            return;
            VertexPositionTexture[] ptCopy = Memory.shadowGeometry.Clone() as VertexPositionTexture[];
            for (int i = 0; i < ptCopy.Length; i++)
                ptCopy[i].Position = Vector3.Transform(ptCopy[i].Position, Matrix.CreateScale(scale));
            for (int i = 0; i < ptCopy.Length; i++)
                ptCopy[i].Position = Vector3.Add(ptCopy[i].Position, new Vector3(enemyPosition.X, 0.1f, enemyPosition.Z));
            ate.Texture = Memory.shadowTexture;
            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ptCopy, 0, 8);
            }
        }

        /// <summary>
        /// Increments animation frames by N, where N is equal to int(deltaTime/FPS). 15FPS is one
        /// frame per ~66 miliseconds. Therefore if deltaTime hits at least: below 33, then frame
        /// gets interpolated above 122, then frame gets skipped (by x/66)
        /// </summary>
        private static void UpdateFrames()
        {
            FrameTime += Memory.gameTime.ElapsedGameTime;
            if (FrameTime > FPS)
            {
                if (Enemy.Party != null)
                    foreach (Enemy e in Enemy.Party)
                    {
                        if (!e.EII.animationSystem.AnimationStopped && !e.IsPetrify)
                            e.EII.animationSystem.NextFrame();
                    }
                //for (int x = 0; x < Enemy.Party.Count; x++)
                //{
                //    EnemyInstanceInformation InstanceInformationProvider = Enemy.Party[x].EII;
                //    InstanceInformationProvider.animationSystem.animationFrame++;
                //    Enemy.Party[x].EII = InstanceInformationProvider;
                //}
                if (CharacterInstances != null)
                    foreach (CharacterInstanceInformation cii in CharacterInstances)
                    {
                        if (!cii.animationSystem.AnimationStopped && (!Memory.State[cii.VisibleCharacter]?.IsPetrify ?? true))
                            cii.animationSystem.NextFrame();
                    }
                //for (int x = 0; x < CharacterInstances.Count; x++)
                //{
                //    CharacterInstanceInformation InstanceInformationProvider = CharacterInstances[x];
                //    InstanceInformationProvider.animationSystem.animationFrame++;
                //    CharacterInstances[x] = InstanceInformationProvider;
                //}
                ResetTime();
            }
        }

        /// <summary>
        /// Plays requested animation for given entity immidiately (without waiting for current
        /// animation to stop if have any queued animations)
        /// </summary>
        /// <param name="entityType">Provide either Monster or Character/weapon</param>
        /// <param name="nIndex">
        /// Index of entityTypeInstance. Monster is monsterInstances, character is CharacterInstances
        /// </param>
        /// <param name="newAnimId">self explanatory</param>
        public static void PlayAnimationImmidiately(Debug_battleDat.EntityType entityType, int nIndex, int newAnimId)
        {
            switch (entityType)
            {
                case Debug_battleDat.EntityType.Monster:
                    EnemyInstanceInformation MInstanceInformationProvider = Enemy.Party[nIndex].EII;
                    MInstanceInformationProvider.animationSystem.AnimationId = newAnimId;
                    Enemy.Party[nIndex].EII = MInstanceInformationProvider;
                    return;

                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    CharacterInstanceInformation CInstanceInformationProvider = CharacterInstances[nIndex];
                    CInstanceInformationProvider.animationSystem.AnimationId = newAnimId;
                    CharacterInstances[nIndex] = CInstanceInformationProvider;
                    return;

                default:
                    return;
            }
        }

        public static void AddAnimationToQueue(Debug_battleDat.EntityType entityType, int nIndex, int newAnimId)
        {
            switch (entityType)
            {
                case Debug_battleDat.EntityType.Monster:
                    Enemy.Party[nIndex].EII.animationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    CharacterInstances[nIndex].animationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                default:
                    return;
            }
        }

        public static void AddSequenceToQueue(Debug_battleDat.EntityType entityType, int nIndex, Debug_battleDat.AnimationSequence section5)
        {
            foreach (byte newAnimId in section5.AnimationQueue)
            {
                AddAnimationToQueue(entityType, nIndex, newAnimId);
            }
        }

        //const float defaultmaxMoveSpeed = 1f;
        //const float MoveSpeedChange = 1f;
        //static float maxMoveSpeed = defaultmaxMoveSpeed;
        //const float maxLookSpeed = 0.25f;

        //public static void FPSCamera()
        //{
        //    #region FPScamera
        //    //speedcontrols
        //    //+ to increase
        //    //- to decrease
        //    //* to reset
        //    if (Input2.Button(Keys.OemPlus) || Input2.Button(Keys.Add))
        //    {
        //        maxMoveSpeed += MoveSpeedChange;
        //    }
        //    if (Input2.Button(Keys.OemMinus) || Input2.Button(Keys.Subtract))
        //    {
        //        maxMoveSpeed -= MoveSpeedChange;
        //        if (maxMoveSpeed < defaultmaxMoveSpeed) maxMoveSpeed = defaultmaxMoveSpeed;
        //    }
        //    if (Input2.Button(Keys.Multiply)) maxMoveSpeed = defaultmaxMoveSpeed;

        // //speed is effected by the milliseconds between frames. so alittle goes a long way. :P
        // Vector2 shift = InputMouse.Distance(MouseButtons.MouseToStick, maxLookSpeed); Vector2
        // leftdist = InputGamePad.Distance(GamePadButtons.LeftStick, maxMoveSpeed).Abs(); shift +=
        // InputGamePad.Distance(GamePadButtons.RightStick, maxMoveSpeed); Yshift -= shift.Y; degrees
        // = (degrees + (int)shift.X) % 360; Yshift = MathHelper.Clamp(Yshift, -80, 80); if (leftdist
        // == Vector2.Zero) { leftdist.Y = (float)Input2.Distance(maxMoveSpeed); leftdist.X =
        // (float)Input2.Distance(maxMoveSpeed); }

        // if (Input2.Button(FF8TextTagKey.Up)) { camPosition.X +=
        // (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdist.Y / 10; camPosition.Z +=
        // (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdist.Y / 10; camPosition.Y -= Yshift
        // / 50; } if (Input2.Button(FF8TextTagKey.Down)) { camPosition.X -=
        // (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdist.Y / 10; camPosition.Z -=
        // (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdist.Y / 10; camPosition.Y += Yshift
        // / 50; } if (Input2.Button(FF8TextTagKey.Left)) { camPosition.X +=
        // (float)Math.Cos(MathHelper.ToRadians(degrees - 90)) * leftdist.X / 10; camPosition.Z +=
        // (float)Math.Sin(MathHelper.ToRadians(degrees - 90)) * leftdist.X / 10; } if
        // (Input2.Button(FF8TextTagKey.Right)) { camPosition.X +=
        // (float)Math.Cos(MathHelper.ToRadians(degrees + 90)) * leftdist.X / 10; camPosition.Z +=
        // (float)Math.Sin(MathHelper.ToRadians(degrees + 90)) * leftdist.X / 10; }

        // //Input.LockMouse();

        //    camTarget.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
        //    camTarget.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
        //    camTarget.Y = camPosition.Y - Yshift / 5;
        //    viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
        //                 Vector3.Up);
        //    #endregion
        //}
        private static void DrawGeometry()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);

            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;

            effect.TextureEnabled = true;
            for (int n = 0; n < modelGroups.Length; n++)
                foreach (Model b in modelGroups[n].models)
                {
                    Tuple<Stage_GeometryInfoSupplier[], VertexPositionTexture[]> vpt = GetVertexBuffer(b);
                    if (n == 3 && skyRotators[Memory.encounters[Memory.battle_encounter].Scenario] != 0)
                        CreateRotation(vpt);
                    if (vpt == null) continue;
                    int localVertexIndex = 0;
                    for (int i = 0; i < vpt.Item1.Length; i++)
                    {
                        ate.Texture = (Texture2D)textures[vpt.Item1[i].clut]; //provide texture per-face
                        foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            if (vpt.Item1[i].bQuad)
                            {
                                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 2);
                                localVertexIndex += 6;
                            }
                            else
                            {
                                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 1);
                                localVertexIndex += 3;
                            }
                        }
                    }
                }

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(new FF8String($"Encounter ready at: {Memory.battle_encounter}"), 0, 0, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Debug variable: {DEBUGframe} ({DEBUGframe >> 4},{DEBUGframe & 0b1111})"), 20, 30 * 1, 1, 1, 0, 1);
            if (Memory.gameTime.ElapsedGameTime.TotalMilliseconds > 0)
                Memory.font.RenderBasicText(new FF8String($"1000/deltaTime milliseconds: {1000 / Memory.gameTime.ElapsedGameTime.TotalMilliseconds}"), 20, 30 * 2, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"camera frame: {battleCamera.cam.CurrentTime}/{battleCamera.cam.TotalTime}"), 20, 30 * 3, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Position: {Extended.RemoveBrackets(camPosition.ToString())}"), 20, 30 * 4, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Target: {Extended.RemoveBrackets(camTarget.ToString())}"), 20, 30 * 5, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.FOV: {MathHelper.Lerp(battleCamera.cam.startingFOV, battleCamera.cam.endingFOV, battleCamera.cam.CurrentTime.Ticks / (float)battleCamera.cam.TotalTime.Ticks)}"), 20, 30 * 6, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.Mode: {battleCamera.cam.control_word & 1}"), 20, 30 * 7, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"DEBUG: Press 0 to switch between FPSCamera/Camera anim: {bUseFPSCamera}"), 20, 30 * 8, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Sequence ID: {SID}, press F10 to activate sequence, F11 SID--, F12 SID++"), 20, 30 * 9, 1, 1, 0, 1);

            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Moves sky
        /// </summary>
        /// <param name="vpt"></param>
        private static void CreateRotation(Tuple<Stage_GeometryInfoSupplier[], VertexPositionTexture[]> vpt)
        {
            localRotator += (short)skyRotators[Memory.encounters[Memory.battle_encounter].Scenario] / 4096f * Memory.gameTime.ElapsedGameTime.Milliseconds;
            if (localRotator <= 0)
                return;
            for (int i = 0; i < vpt.Item2.Length; i++)
                vpt.Item2[i].Position = Vector3.Transform(vpt.Item2[i].Position, Matrix.CreateRotationY(MathHelper.ToRadians(localRotator)));
        }

        /// <summary>
        /// Converts requested Model data (Stage group geometry) into MonoGame VertexPositionTexture
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static Tuple<Stage_GeometryInfoSupplier[], VertexPositionTexture[]> GetVertexBuffer(Model model)
        {
            List<VertexPositionTexture> vptDynamic = new List<VertexPositionTexture>();
            List<Stage_GeometryInfoSupplier> bs_renderer_supplier = new List<Stage_GeometryInfoSupplier>();
            if (model.vertices == null) return null;
            for (int i = 0; i < model.triangles.Length; i++)
            {
                Vertex A = model.vertices[model.triangles[i].A];
                Vertex B = model.vertices[model.triangles[i].B];
                Vertex C = model.vertices[model.triangles[i].C];
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.triangles[i].U2, model.triangles[i].V2, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.triangles[i].U3, model.triangles[i].V3, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.triangles[i].U1, model.triangles[i].V1, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                bs_renderer_supplier.Add(new Stage_GeometryInfoSupplier()
                {
                    bQuad = false,
                    clut = model.triangles[i].clut,
                    texPage = model.triangles[i].TexturePage
                });
            }
            for (int i = 0; i < model.quads.Length; i++)
            {
                //I have to re-trangulate it. Fortunately I had been working on this lately
                Vertex A = model.vertices[model.quads[i].A]; //1
                Vertex B = model.vertices[model.quads[i].B]; //2
                Vertex C = model.vertices[model.quads[i].C]; //4
                Vertex D = model.vertices[model.quads[i].D]; //3

                //triangluation wing-reorder
                //1 2 4
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.quads[i].U2, model.quads[i].V2, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, textureInterface.GetWidth)));

                //1 3 4
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.quads[i].U3, model.quads[i].V3, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, textureInterface.GetWidth)));

                bs_renderer_supplier.Add(new Stage_GeometryInfoSupplier()
                {
                    bQuad = true,
                    clut = model.quads[i].clut,
                    texPage = model.quads[i].TexturePage
                });
            }
            return new Tuple<Stage_GeometryInfoSupplier[], VertexPositionTexture[]>
                (bs_renderer_supplier.ToArray(), vptDynamic.ToArray());
        }

        private static Vector2 CalculateUV(byte U, byte V, byte texPage, int texWidth)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            float fU = (float)U / texWidth + (((float)texPage * 128) / texWidth);
            float fV = V / 256.0f;
            return new Vector2(fU, fV);
        }

        private static Vector3 PyramidOffset = new Vector3(0, 3f, 0);

        private static void InitBattle()
        {
            //MakiExtended.Debugger_Spawn();
            //MakiExtended.Debugger_Feed(typeof(Module_battle_debug), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            InputMouse.Mode = MouseLockMode.Center;
            if (DeadTime == null)
            {
                DeadTime = new DeadTime();
                DeadTime.DoneEvent += DeadTime_DoneEvent;
            }
            DeadTime.Restart();
            fps_camera = new FPS_Camera();
            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            int stage = enc.Scenario;
            battlename = $"a0stg{stage.ToString("000")}.x";
            Console.WriteLine($"BS_DEBUG: Loading stage {battlename}");
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.battle_encounter}\t cEnemies: {enc.EnabledEnemy}\t Enemies: {string.Join(",", enc.BEnemies.Where(x => x != 0x00).Select(x => $"{x}").ToArray())}");
            RegularPyramid = new Battle.RegularPyramid();
            RegularPyramid.Set(-2.5f, 2f, Color.Yellow);
            //RegularPyramid.Set(PyramidOffset);
            RegularPyramid.Hide();
            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            camTarget = new Vector3(41.91198f, 33.59995f, 6.372305f);
            camPosition = new Vector3(40.49409f, 39.70397f, -43.321299f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);
            battleModule++;
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Projection = projectionMatrix,
                View = viewMatrix,
                World = worldMatrix
            };
            return;
        }

        /// <summary>
        /// Trigger Event when DeadTime is done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936"/>
        private static void DeadTime_DoneEvent(object sender, int e)
        {
            //Will Gilgamesh appear?
            if (TestGilgamesh())
            { }
            //Will Angelo Recover be used?
            else if (TestAngelo(Angelo.Recover))
            { }
            //Will Angelo Reverse be used?
            else if (TestAngelo(Angelo.Reverse))
            { }
            //Will Angelo Search be used?
            else if (TestAngelo(Angelo.Search))
            {
                //Real game has a counter that count to 255 and resets to 0
                //instead of a random number. The counter counts up every 1 tick.
                //60 ticks per second.
                byte rnd = checked((byte)Memory.Random.Next(256));
                if (rnd < 128) Algorithm(1);
                else if (rnd < 160) Algorithm(2);
                else if (rnd < 176) Algorithm(3);
                else if (rnd < 192) Algorithm(4);
                else if (rnd < 200) Algorithm(5);
                else Algorithm(6);
                Saves.Item Algorithm(byte i)
                {
                    //https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/58936
                    //I'm unsure where in the game files this is.
                    //In remaster they changed this. But unsure how
                    // they added (Ribbon, Friendship, and Mog's Amulet)
                    // using a true random kinda breaks this.
                    // because in game random is a set array of numbers 0-255
                    // so the number you get previous would determin the possible number you get
                    // so these can only select specific numbers. But because we are using a real random
                    // more items are possible. might need to tweak this.

#pragma warning disable CS0219 // Variable is assigned but its value is never used
                    //these are added in remaster as possible items.
                    const byte Ribbon = 100;
                    const byte Friendship = 32;
                    const byte Mogs_Amulet = 65;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

                    Saves.Item item = new Saves.Item { QTY = 1 };
                    rnd = checked((byte)Memory.Random.Next(256));
                    switch (i)
                    {
                        case 1: // 1-8
                            item.ID = (byte)(rnd % 8 + 1);
                            break;

                        case 2: // 102-199
                            item.ID = (byte)(rnd % 98);
                            if (item.ID == 0) item.ID = 98;
                            item.ID += 101;
                            break;

                        case 3: // 102-124
                            item.ID = (byte)(rnd % 23);
                            if (item.ID == 0)
                                item.ID = 23;
                            item.ID += 101;
                            break;

                        case 4: // 67-100
                            item.ID = (byte)(rnd % 34);
                            if (item.ID == 0)
                                item.ID = 34;
                            item.ID += 66;
                            break;

                        case 5: // 33-54
                            item.ID = (byte)(rnd % 32 + 33);
                            break;

                        case 6:
                        default: // 33-40
                            item.ID = (byte)(rnd % 7 + 33);
                            break;
                    }
                    return item;
                }
            }
        }

        private static bool TestAngelo(Angelo ability)
        {
            //else if (8 >= [0..255] Angelo Recover is used (3.3 %)
            //else if (2 >= [0..255] Angelo Reverse is used (1 %)
            //else if (8 >= [0..255] Angelo Search is used (3.2 %)
            //Angelo_Disabled I think is set when Rinoa is in space so angelo is out of reach;
            //https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/25194
            if (Memory.State.BattleMISCIndicator.HasFlag(Saves.Data.MiscIndicator.Angelo_Disabled) ||
                !Memory.State.PartyData.Contains(Characters.Rinoa_Heartilly) ||
                !Memory.State.LimitBreakAngelocompleted.HasFlag(ability)) return false;
            else
                switch (ability)
                {
                    case Angelo.Recover:
                        return Memory.State.Characters.Any(x => x.Value.IsCritical && !x.Value.IsDead && Memory.State.PartyData.Contains(x.Key)) && Memory.Random.Next(256) < 8;

                    case Angelo.Reverse:
                        return Memory.State.Characters.Any(x => x.Value.IsDead && Memory.State.PartyData.Contains(x.Key)) && Memory.Random.Next(256) < 2;

                    case Angelo.Search:
                        Saves.CharacterData c = Memory.State[Characters.Rinoa_Heartilly];
                        if (!(c.IsGameOver ||
                            c.Statuses1.HasFlag(Kernel_bin.Battle_Only_Statuses.Sleep) ||
                            c.Statuses1.HasFlag(Kernel_bin.Battle_Only_Statuses.Stop) ||
                            c.Statuses1.HasFlag(Kernel_bin.Battle_Only_Statuses.Confuse) ||
                            c.Statuses1.HasFlag(Kernel_bin.Persistent_Statuses.Berserk) ||
                            c.Statuses1.HasFlag(Kernel_bin.Battle_Only_Statuses.Angel_Wing)))
                            return Memory.Random.Next(256) < 8;
                        break;
                }
            return false;
        }

        private static bool TestGilgamesh() =>

            //if (12 >= [0..255]) Gilgamesh is summoned (5.1 %)
            Memory.State.BattleMISCIndicator.HasFlag(Saves.Data.MiscIndicator.Gilgamesh) && Memory.Random.Next(256) < 12;

        #region fileParsing

        private static List<Battle.Mag> MagALL;

        static IEnumerable<Battle.Mag> MagTIMs => MagALL?.Where(x => x.isTIM) ?? null;
        static IEnumerable<Battle.Mag> MagPacked => MagALL?.Where(x => x.isPackedMag) ?? null;
        static IEnumerable<Battle.Mag> MagGeometries => MagALL?.Where(x => (x.Geometries?.Count??0) >0) ?? null;
        static IEnumerable<int> MagUNKID => MagALL?.Where(x => x.UnknownType >0).Select(x=>x.UnknownType) ?? null;

        private static void ReadData()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] test = aw.GetListOfFiles();
            MagALL = new List<Battle.Mag>();
            foreach (string filename in test.Where(x => x.IndexOf("mag", StringComparison.OrdinalIgnoreCase) > 0))
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, filename))))
                {
                    Battle.Mag mag = new Battle.Mag(filename, br);
                    MagALL.Add(mag);
                }
            }
            var _MagGeo = MagGeometries.ToList();
            var _MagPack = MagPacked.ToList();
            var _MagTIM = MagTIMs.ToList();
            var _MagUNKID = MagUNKID.ToList();
            battlename = test.First(x => x.ToLower().Contains(battlename));
            string fileName = Path.GetFileNameWithoutExtension(battlename);
            stageBuffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, battlename);
            ms = new MemoryStream(stageBuffer);
            br = new BinaryReader(ms);
            bs_cameraPointer = GetCameraPointer();
            ms.Seek(bs_cameraPointer, 0);
            ReadCamera();
            uint sectionCounter = br.ReadUInt32();
            if (sectionCounter != 6)
            {
                Console.WriteLine($"BS_PARSER_PRE_OBJECTSECTION: Main geometry section has no 6 pointers at: {ms.Position}");
                battleModule++;
                return;
            }
            MainGeometrySection MainSection = ReadObjectGroupPointers();
            ObjectsGroup[] objectsGroups = new ObjectsGroup[4]
            {
                ReadObjectsGroup(MainSection.Group1Pointer),
                ReadObjectsGroup(MainSection.Group2Pointer),
                ReadObjectsGroup(MainSection.Group3Pointer),
                ReadObjectsGroup(MainSection.Group4Pointer)
            };

            modelGroups = new ModelGroup[4]
            {
                ReadModelGroup(objectsGroups[0].objectListPointer),
                ReadModelGroup(objectsGroups[1].objectListPointer),
                ReadModelGroup(objectsGroups[2].objectListPointer),
                ReadModelGroup(objectsGroups[3].objectListPointer)
            };

            ReadTexture(MainSection.TexturePointer, fileName);
            br.Close();
            ms.Close();
            ms.Dispose();

            ReadCharacters();
            ReadMonster();

            battleModule++;
        }

        public static int DEBUG = 0;
        private static TimeSpan FrameTime = TimeSpan.Zero;

        private static void ResetTime() => FrameTime = TimeSpan.FromTicks(FrameTime.Ticks % FPS.Ticks);

        public static ConcurrentDictionary<Characters, SortedSet<byte>> Costumes { get; private set; }

        private static void FillCostumes()
        {
            if (Costumes == null)
            {
                Costumes = new ConcurrentDictionary<Characters, SortedSet<byte>>();
                Regex r = new Regex(@"d([\da-fA-F]+)c(\d+)\.dat", RegexOptions.IgnoreCase);
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
                foreach (string s in aw.FileList)
                {
                    Match match = r.Match(s);
                    if (match != null)
                    {
                        Characters c = Characters.Blank;
                        if (byte.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte ci))
                        {
                            c = (Characters)ci;

                            if (byte.TryParse(match.Groups[2].Value, out byte a))
                            {
                                Costumes.TryAdd(c, new SortedSet<byte>());
                                Costumes[c].Add(a);
                            }
                        }
                    }
                }
            }
        }

        public static ConcurrentDictionary<Characters, List<byte>> Weapons
        {
            get
            {
                FillWeapons();
                return s_weapons;
            }
            private set => s_weapons = value;
        }

        private static void FillWeapons()
        {
            if (s_weapons == null)
            {
                Weapons = new ConcurrentDictionary<Characters, List<byte>>();
                for (int i = 0; i <= (int)Characters.Ward_Zabac; i++)
                {
                    SortedSet<byte> _weapons = new SortedSet<byte>();
                    Regex r = new Regex(@"d(" + i.ToString("X") + @")w(\d+)\.dat", RegexOptions.IgnoreCase);
                    ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);

                    foreach (string s in aw.FileList.OrderBy(q => Path.GetFileName(q), StringComparer.InvariantCultureIgnoreCase))
                    {
                        Match match = r.Match(s);
                        if (match != null)
                        {
                            if (byte.TryParse(match.Groups[2].Value, out byte a))
                            {
                                _weapons.Add(a);
                            }
                        }
                    }
                    Weapons.TryAdd((Characters)i, _weapons.ToList());
                }
            }
        }

        /// <summary>
        /// Main Animations IDs
        /// </summary>
        /// <remarks>more beyond this maybe part of attacking and such.</remarks>
        /// <see cref="http://forums.qhimm.com/index.php?topic=19362.msg269777#msg269777"/>
        private enum AnimID : int
        {
            Idle = 0,
            Critical = 1,
            Dead = 2,
            //These seem to be referring to something else.
            //Like he says theres another section with 30 sequences.
            //where it tells how to chain the animations together.

            //Damage_Taken_low_hp,
            //Damage_Taken_normal,
            //Damage_Taken_crit,
            //Nothing,
            //Appearance,
            //Ready_to_Attack,
            //Fail_Draw,
            //Magic,
            //Standing,
            //Attack_normal,
            //SummonGF_hide,
            //Item_Use,
            //Escape,
            //Escaped_vanish,
            //Victory,
            //Becoming_Ready_to_Attack,
            //SummonGF_show,
            //Limit_break_normal,
            //Draw_Defend_Phase_again,
            //Becoming_Defend_Draw_Phase,
            //Kamikaze_Command,
            //Attack_Darkside,
            //Escape_2,
            //Defend_Draw_stock,
            //Limit_break_Special,
            //Defend_command_standing_again,
            //Draw_Stock_Magic
        }

        /// <summary>
        /// [WIP] Basic class responsible for creating character model into game. It should be
        /// changed to be parsed from current party
        /// </summary>
        private static void ReadCharacters()
        {
            if (Memory.State?.Characters != null)
            {
                FillCostumes();
                FillWeapons();
                CharacterInstances = new List<CharacterInstanceInformation>(3);
                int cid = 0;
                foreach (Characters c in Memory.State.Party)
                {
                    if (c != Characters.Blank)
                    {
                        byte weaponId = 0;
                        if (Memory.State.Characters.TryGetValue(c, out Saves.CharacterData characterData) &&
                            characterData.WeaponID < Kernel_bin.WeaponsData.Count)
                        {
                            byte altID = Kernel_bin.WeaponsData[characterData.WeaponID].AltID;
                            if (Weapons.TryGetValue(c, out List<byte> weapons) && weapons != null && weapons.Count > altID)
                                weaponId = weapons[altID];
                        }

                        CharacterInstanceInformation cii = new CharacterInstanceInformation
                        {
                            Data = ReadCharacterData((int)c,
                                Memory.State[c].Alternativemodel == 0 ? Costumes[c].First() : Costumes[c].Last(),
                                weaponId),
                            animationSystem = new AnimationSystem() { AnimationQueue = new ConcurrentQueue<int>() },
                            characterId = cid++,
                        };
                        //cii.animationSystem.animationId = 4;
                        Memory.State[c].BattleStart(cii);
                        CharacterInstances.Add(cii);
                    }
                }
            }
            else
                CharacterInstances = new List<CharacterInstanceInformation>
            {
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(0,0,0),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new ConcurrentQueue<int>()},
                    characterId = 0,
                },
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(1,3,8),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new ConcurrentQueue<int>()},
                    characterId = 1
                },
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(2,6,13),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new ConcurrentQueue<int>()},
                    characterId = 2
                }
            };
        }

        private static CharacterData ReadCharacterData(int characterId, int alternativeCostumeId, int weaponId)
        {
            Debug_battleDat character = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Character, alternativeCostumeId);
            Debug_battleDat weapon;
            if (characterId == 1 || characterId == 9)
                weapon = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Weapon, weaponId, character);
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (weaponId != -1) weapon = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Weapon, weaponId);
#pragma warning restore IDE0045 // Convert to conditional expression
            else weapon = null;
            return new CharacterData()
            {
                character = character,
                weapon = weapon
            };
        }

        /// <summary>
        /// This method is responsible to read/parse the enemy data. It holds the result in
        /// monstersData[] This method was designed to read only one instance of enemy. A list called
        /// EnemyInstance holds data information for each enemy
        /// </summary>
        private static void ReadMonster()
        {
            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            if (enc.EnabledEnemy == 0)
            {
                monstersData = new Debug_battleDat[0];
                return;
            }
            List<byte> monstersList = enc.BEnemies.ToList();
            for (int i = 7; i > 0; i--)
            {
                bool bEnabled = Extended.GetBit(enc.EnabledEnemy, 7 - i);
                if (!bEnabled)
                    monstersList.RemoveLast();
            }
            IGrouping<byte, byte>[] DistinctMonsterPointers = monstersList.GroupBy(x => x).ToArray();
            monstersData = new Debug_battleDat[DistinctMonsterPointers.Count()];
            for (int n = 0; n < monstersData.Length; n++)
                monstersData[n] = Debug_battleDat.Load(DistinctMonsterPointers[n].Key, Debug_battleDat.EntityType.Monster);
            if (monstersData == null)
                monstersData = new Debug_battleDat[0];
            Enemy.Party = new List<Enemy>(8);
            //EnemyInstances = new List<EnemyInstanceInformation>();
            for (int i = 0; i < 8; i++)
                if (Extended.GetBit(enc.EnabledEnemy, 7 - i))
                    Enemy.Party.Add(new EnemyInstanceInformation()
                    {
                        Data = monstersData.Where(x => x.GetId == enc.BEnemies[i]).First(),
                        bIsHidden = Extended.GetBit(enc.HiddenEnemies, 7 - i),
                        bIsActive = true,
                        index = (byte)(7 - i),
                        bIsUntargetable = Extended.GetBit(enc.UntargetableEnemy, 7 - i),
                        animationSystem = new AnimationSystem() { AnimationQueue = new ConcurrentQueue<int>() }
                    }
                        );
        }

        /// <summary>
        /// Method designed for Stage texture loading.
        /// </summary>
        /// <param name="texturePointer">Absolute pointer to TIM texture header in stageBuffer</param>
        private static void ReadTexture(uint texturePointer, string fileName)
        {
            textureInterface = new TIM2(stageBuffer, texturePointer);
            textures = new TextureHandler[textureInterface.GetClutCount];
            for (ushort i = 0; i < textureInterface.GetClutCount; i++)
                textures[i] = TextureHandler.Create(fileName, textureInterface, i);
        }

        /// <summary>
        /// Reads Stage model groups pointers and reads/parses them individually. Group0 is stage
        /// ground. It's always enabled except special sequences like GFs Group1 is main geometry.
        /// It's prior to Time Compression deformation Group2 is main/additional geometry. It's prior
        /// to Time Compression deformation Group3 is Sky. It's NON-prior to Time Compression, but
        /// may be modified by SkyRotators and/or TimeCompression last Stage skyRotation multiplier
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        private static ModelGroup ReadModelGroup(uint pointer)
        {
            ms.Seek(pointer, SeekOrigin.Begin);
            uint modelsCount = br.ReadUInt32();
            Model[] models = new Model[modelsCount];
            uint[] modelPointers = new uint[modelsCount];
            for (int i = 0; i < modelsCount; i++)
                modelPointers[i] = pointer + br.ReadUInt32();
            for (int i = 0; i < modelsCount; i++)
                models[i] = ReadModel(modelPointers[i]);
            return new ModelGroup() { models = models };
        }

        /// <summary>
        /// This is the main class that reads given Stage geometry group. It stores the data into
        /// Model structure
        /// </summary>
        /// <param name="pointer">absolute pointer in buffer for given Stage geometry group</param>
        /// <returns></returns>
        private static Model ReadModel(uint pointer)
        {
            bool bSpecial = false;
            ms.Seek(pointer, System.IO.SeekOrigin.Begin);
            uint header = Extended.UintLittleEndian(br.ReadUInt32());
            if (header != 0x01000100) //those may be some switches, but I don't know what they mean
            {
                Console.WriteLine("WARNING- THIS STAGE IS DIFFERENT! It has weird object section. INTERESTING, TO REVERSE!");
                bSpecial = true;
            }
            ushort verticesCount = br.ReadUInt16();
            Vertex[] vertices = new Vertex[verticesCount];
            for (int i = 0; i < verticesCount; i++)
                vertices[i] = ReadVertex();
            if (bSpecial && Memory.encounters[Memory.battle_encounter].Scenario == 20)
                return new Model();
            ms.Seek((ms.Position % 4) + 4, SeekOrigin.Current);
            ushort trianglesCount = br.ReadUInt16();
            ushort quadsCount = br.ReadUInt16();
            ms.Seek(4, SeekOrigin.Current);
            Triangle[] triangles = new Triangle[trianglesCount];
            Quad[] quads = new Quad[quadsCount];
            if (trianglesCount > 0)
                for (int i = 0; i < trianglesCount; i++)
                    triangles[i] = ReadTriangle();
            if (quadsCount > 0)
                for (int i = 0; i < quadsCount; i++)
                    quads[i] = ReadQuad();
            return new Model()
            {
                vertices = vertices,
                triangles = triangles,
                quads = quads
            };
        }

        private static Triangle ReadTriangle()
        =>
            new Triangle()
            {
                A = br.ReadUInt16(),
                B = br.ReadUInt16(),
                C = br.ReadUInt16(),
                U1 = br.ReadByte(),
                V1 = br.ReadByte(),
                U2 = br.ReadByte(),
                V2 = br.ReadByte(),
                clut = GetClutId(br.ReadUInt16()),
                U3 = br.ReadByte(),
                V3 = br.ReadByte(),
                TexturePage = GetTexturePage(br.ReadByte()),
                bHide = br.ReadByte(),
                Red = br.ReadByte(),
                Green = br.ReadByte(),
                Blue = br.ReadByte(),
                GPU = br.ReadByte(),
            };

        private static Quad ReadQuad()
        => new Quad()
        {
            A = br.ReadUInt16(),
            B = br.ReadUInt16(),
            C = br.ReadUInt16(),
            D = br.ReadUInt16(),
            U1 = br.ReadByte(),
            V1 = br.ReadByte(),
            clut = GetClutId(br.ReadUInt16()),
            U2 = br.ReadByte(),
            V2 = br.ReadByte(),
            TexturePage = GetTexturePage(br.ReadByte()),
            bHide = br.ReadByte(),
            U3 = br.ReadByte(),
            V3 = br.ReadByte(),
            U4 = br.ReadByte(),
            V4 = br.ReadByte(),
            Red = br.ReadByte(),
            Green = br.ReadByte(),
            Blue = br.ReadByte(),
            GPU = br.ReadByte()
        };

        private static Vertex ReadVertex()
        => new Vertex()
        {
            X = br.ReadInt16(),
            Y = br.ReadInt16(),
            Z = br.ReadInt16(),
        };

        private static ObjectsGroup ReadObjectsGroup(uint pointer)
        {
            ms.Seek(pointer, System.IO.SeekOrigin.Begin);
            return new ObjectsGroup()
            {
                numberOfSections = br.ReadUInt32(),
                settings1Pointer = pointer + br.ReadUInt32(),
                objectListPointer = pointer + br.ReadUInt32(),
                settings2Pointer = pointer + br.ReadUInt32(),
                relativeEOF = pointer + br.ReadUInt32(),
            };
        }

        private static MainGeometrySection ReadObjectGroupPointers()
        {
            int basePointer = (int)ms.Position - 4;
            uint objectGroup_1 = (uint)basePointer + br.ReadUInt32();
            uint objectGroup_2 = (uint)basePointer + br.ReadUInt32();
            uint objectGroup_3 = (uint)basePointer + br.ReadUInt32();
            uint objectGroup_4 = (uint)basePointer + br.ReadUInt32();
            uint TextureUnused = (uint)basePointer + br.ReadUInt32();
            uint Texture = (uint)basePointer + br.ReadUInt32();
            uint EOF = (uint)basePointer + br.ReadUInt32();
            if (EOF != ms.Length)
                throw new Exception("BS_PARSER_ERROR_LENGTH: Geometry EOF pointer is other than buffered filesize");

            return new MainGeometrySection()
            {
                Group1Pointer = objectGroup_1,
                Group2Pointer = objectGroup_2,
                Group3Pointer = objectGroup_3,
                Group4Pointer = objectGroup_4,
                TextureUNUSEDPointer = TextureUnused,
                TexturePointer = Texture,
                EOF = EOF
            }; //EOF = EOF; beauty of language
        }

        /// <summary>
        /// Gets vanilla engine camera pointers. A team that rewrote the game into PC just left
        /// PlayStation MIPS data inside files and therefore their code is to skip given hardcoded
        /// data which in fact are PS compiled instructions This data is naturally read by
        /// PlayStation in original console release.
        /// </summary>
        /// <returns>Camera pointer (data after PlayStation MIPS)</returns>
        private static uint GetCameraPointer()
        {
            int[] _x5D4 = {4,5,9,12,13,14,15,21,22,23,24,26,
29,32,33,34,35,36,39,40,50,53,55,61,62,63,64,65,66,67,68,69,70,
71,72,73,75,78,82,83,85,86,87,88,89,90,91,94,96,97,98,99,100,105,
106,121,122,123,124,125,126,127,135,138,141,144,145,148,149,150,
151,158,160};

            int[] _x5D8 = {
0,1,2,3,6,7,10,11,17,18,25,27,28,38,41,42,43,47,49,57,58,59,60,74,
76,77,80,81,84,93,95,101,102,103,104,109,110,111,112,113,114,115,116,
117,118,119,120,128,129,130,131,132,133,134,139,140,143,146,152,153,154,
155,156,159,161,162};

            int _5d4 = _x5D4.Count(x => x == Memory.encounters[Memory.battle_encounter].Scenario);
            int _5d8 = _x5D8.Count(x => x == Memory.encounters[Memory.battle_encounter].Scenario);
            if (_5d4 > 0) return 0x5D4;
            if (_5d8 > 0) return 0x5D8;
            switch (Memory.encounters[Memory.battle_encounter].Scenario)
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

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1092)]
        public struct CameraStruct
        {
            public byte animationId; //000
            public byte keyframeCount;
            public ushort control_word;
            public ushort startingFOV; //usually ~280
            public ushort endingFOV; //006
            public ushort startingCameraRoll; //usually 0 unless you're aiming for some wicked animation
            public ushort endingCameraRoll; //
            private ushort startingTime; //usually 0, that's pretty logical

            /// <summary>
            /// Time is calculated from number of frames. You basically set starting position
            /// World+lookat and ending position, then mark number of frames to interpolate between
            /// them. Every frame is one drawcall and it costs 16.
            /// </summary>
            public ushort time; //starting time needs to be equal or higher for next animation frame to be read; If next frame==0xFFFF then it's all done

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] unk; //010

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public ushort[] startFramesOffsets; //024 - start frames for each key frame?

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Z_s16; //064

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_X_s16; //0A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Y_s16; //0E4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] is_FrameDurationsShot; //124

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Z_s16; //144

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_X_s16; //184

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Y_s16; //1C4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] is_FrameEndingShots; //204

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte224; //224

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte2A4; //2A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte324; //324

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte3A4; //3A4

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte424; //424

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] unkbyte4A4; //4A4
            private const float V = 100f;
            Vector3 offset => new Vector3(30, +40, 0);

            public Vector3 Camera_World(int i) => new Vector3(
                Camera_World_X_s16[i] / V, 
                -(Camera_World_Y_s16[i] / V),
                -(Camera_World_Z_s16[i] / V))+offset;
            public Vector3 Camera_Lookat(int i) => new Vector3(
                Camera_Lookat_X_s16[i] / V, 
                -(Camera_Lookat_Y_s16[i] / V), 
                -(Camera_Lookat_Z_s16[i] / V))+offset;
            public void UpdateTime() => CurrentTime += Memory.gameTime.ElapsedGameTime;
            public TimeSpan CurrentTime;
            public TimeSpan TotalTime => TimeSpan.FromTicks(TotalTimePerFrame.Ticks * time);
            /// <summary>
            /// (1000) milliseconds / frames per second
            /// </summary>
            public TimeSpan TotalTimePerFrame => TimeSpan.FromMilliseconds(1000d / 240d);
            
        };

        /// <summary>
        /// Main battle camera struct. It holds data for camera settings, camera collections and main
        /// CameraStruct used as a final result of camera parsing
        /// </summary>
        private struct BattleCamera
        {
            public BattleCameraSettings battleCameraSettings;
            public BattleCameraCollection battleCameraCollection;
            public CameraStruct cam;
            public uint lastCameraPointer;
            public bool bMultiShotAnimation;
        }

        private static BattleCamera battleCamera;
        private static bool bUseFPSCamera = false;
        private static ConcurrentDictionary<Characters, List<byte>> s_weapons;

        /// <summary>
        /// Battle camera settings are about 32 bytes of unknown flags and variables used in whole
        /// stage including geometry
        /// </summary>
        private struct BattleCameraSettings
        {
            public byte[] unk;
        }

        /// <summary>
        /// Main struct for collection of camera animations. Every BattleCameraSet hold 8 animations
        /// no matter what
        /// </summary>
        private struct BattleCameraCollection
        {
            public uint cAnimCollectionCount;
            public uint pCameraEOF;
            public BattleCameraSet[] battleCameraSet;
        }

        /// <summary>
        /// Struct for battle camera animation set. Animation set always contain 8 animations. This
        /// struct does not contain a data for pre-readed information. Therefore you have to call
        /// ReadAnimation(index) to actually read the animation to BattleCamera.cam(CameraStruct).
        /// That is because there are extreme amount of cases where the camera is changing and
        /// reading again and again not including the battle stage. Also reading all camera
        /// animations is waste of time and resources
        /// </summary>
        private struct BattleCameraSet
        {
            public uint[] animPointers;
            public uint globalSetPointer;
        }

        /// <summary>
        /// Parses camera data into BattleCamera struct. Main purpouse of this function is to
        /// actually read all the offsets and pointers to human readable form of struct. This
        /// function later calls ReadAnimation(n) where n is animation Id (i.e. 9 is camCollection=1
        /// and cameraAnim=0)
        /// </summary>
        private static void ReadCamera()
        {
            uint cCameraHeaderSector = br.ReadUInt16();
            uint pCameraSetting = br.ReadUInt16();
            uint pCameraAnimationCollection = br.ReadUInt16();
            uint sCameraDataSize = br.ReadUInt16();

            //Camera settings parsing?
            BattleCameraSettings bcs = new BattleCameraSettings() { unk = br.ReadBytes(24) };
            //end of camera settings parsing

            ms.Seek(bs_cameraPointer, 0);
            ms.Seek(pCameraAnimationCollection, SeekOrigin.Current);
            BattleCameraCollection bcc = new BattleCameraCollection { cAnimCollectionCount = br.ReadUInt16() };
            BattleCameraSet[] bcset = new BattleCameraSet[bcc.cAnimCollectionCount];
            bcc.battleCameraSet = bcset;
            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
                bcset[i] = new BattleCameraSet() { globalSetPointer = (uint)(ms.Position + br.ReadUInt16() - i * 2 - 2) };
            bcc.pCameraEOF = br.ReadUInt16();

            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
            {
                ms.Seek(bcc.battleCameraSet[i].globalSetPointer, 0);
                bcc.battleCameraSet[i].animPointers = new uint[8];
                for (int n = 0; n < bcc.battleCameraSet[i].animPointers.Length; n++)
                    bcc.battleCameraSet[i].animPointers[n] = (uint)(ms.Position + br.ReadUInt16() * 2 - n * 2);
            }
            CameraStruct cam = Extended.ByteArrayToStructure<CameraStruct>(new byte[Marshal.SizeOf(typeof(CameraStruct))]); //what about this kind of trick to initialize struct with a lot amount of fixed sizes in arrays?
            battleCamera = new BattleCamera() { battleCameraCollection = bcc, battleCameraSettings = bcs, cam = cam };

            ReadAnimationById(GetRandomCameraN(Memory.encounters[Memory.battle_encounter]));
            ms.Seek(bs_cameraPointer + sCameraDataSize, 0); //step out
        }

        /// <summary>
        /// Gets random camera from available from encounter- primary or secondary
        /// </summary>
        /// <param name="encounter">instance of current encounter</param>
        /// <returns>Either primary or alternative camera from encounter</returns>
        private static byte GetRandomCameraN(Init_debugger_battle.Encounter encounter)
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

        private struct CameraSetAnimGRP
        {
            public int Set { get; private set; }
            public int Anim { get; private set; }

            public CameraSetAnimGRP(int set, int anim)
            {
                Set = set;
                Anim = anim;
            }
        }

        /// <summary>
        /// Returns tuple containing camera animation set pointer and camera animation in that set
        /// </summary>
        /// <param name="animId">6bit variable containing camera pointer</param>
        /// <returns>Tuple with CameraSetPointer, CameraSetPointer[CameraAnimationPointer]</returns>
        private static CameraSetAnimGRP GetCameraCollectionPointers(byte animId)
        {
            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            int pSet = enc.ResolveCameraSet(animId);
            int pAnim = enc.ResolveCameraAnimation(animId);
            return new CameraSetAnimGRP(pSet, pAnim);
        }

        /// <summary>
        /// This method resolves the correct camera pointer and runs ReadAnimation(uint,ms,br) method
        /// and returns the final pointer
        /// </summary>
        /// <param name="animId">
        /// Animation Id as of binary mask (0bXXXXYYYY where XXXX= animationSet and YYYY=animationId)
        /// </param>
        /// <returns></returns>
        private static uint ReadAnimationById(byte animId)
        {
            animId = (byte)DEBUGframe; //DEBUG
            CameraSetAnimGRP tpGetter = GetCameraCollectionPointers(animId);
            BattleCameraSet[] battleCameraSetArray = battleCamera.battleCameraCollection.battleCameraSet;
            if (battleCameraSetArray.Length > tpGetter.Set && battleCameraSetArray[tpGetter.Set].animPointers.Length > tpGetter.Anim)
            {
                BattleCameraSet battleCameraSet = battleCameraSetArray[tpGetter.Set];
                uint cameraAnimationGlobalPointer = battleCameraSet.animPointers[tpGetter.Anim];
                return ReadAnimation(cameraAnimationGlobalPointer);
            }
            else
            {
                Debug.WriteLine($"ReadAnimationById::{battleCameraSetArray.Length} < {tpGetter.Set}" +
                    $"\nor\n" +
                    $"{battleCameraSetArray[tpGetter.Set].animPointers.Length} < {tpGetter.Anim}");
            }
            return 0;
        }

        /// <summary>
        /// This method reads raw animation data in stage file or if ms_ and br_ == null then ms_
        /// file and stores parsed data into battleCamera struct
        /// </summary>
        /// <param name="cameraAnimOffset">
        /// if (ms_ and br_ are null) is an offset in current battle stage file for camera animation.
        /// If ms_ and _br are provided it's the offset in this file
        /// </param>
        /// <param name="ms_">if null then stage file either this memory stream</param>
        /// <param name="br_">sub-component of ms</param>
        /// <remarks See also = BS_Camera_ReadAnimation - 00503AC0></remarks>
        /// <returns></returns>
        private static uint ReadAnimation(uint cameraAnimOffset, MemoryStream ms_ = null, BinaryReader br_ = null)
        {
            if (ms_ == null || br_ == null)
            { ms_ = ms; br_ = br; }

            short local2C;
            byte keyframecount = 0;
            ushort totalframecount = 0;
            short local1C;
            short local18;
            short local14;
            short local10;

            ms_.Seek(cameraAnimOffset, SeekOrigin.Begin);
            battleCamera.cam.control_word = br_.ReadUInt16();
            if (battleCamera.cam.control_word == 0xFFFF)
                return 0; //return NULL

            ushort current_position = br_.ReadUInt16(); //getter for *current_position
            ms_.Seek(-2, SeekOrigin.Current); //roll back one WORD because no increment

            switch ((battleCamera.cam.control_word >> 6) & 3)
            {
                case 1:
                    battleCamera.cam.startingFOV = 0x200;
                    battleCamera.cam.endingFOV = 0x200;
                    break;

                case 2:
                    battleCamera.cam.startingFOV = current_position;
                    battleCamera.cam.endingFOV = current_position;
                    br_.ReadUInt16(); //current_position++
                    break;

                case 3:
                    battleCamera.cam.startingFOV = current_position;
                    ms_.Seek(2, SeekOrigin.Current); //skipping WORD, because we already rolled back one WORD above this switch
                    current_position = br_.ReadUInt16();
                    battleCamera.cam.endingFOV = current_position;
                    break;
            }
            switch ((battleCamera.cam.control_word >> 8) & 3)
            {
                case 0: //TODO!!
                    battleCamera.cam.startingCameraRoll = 00000000; //TODO, what's ff8vars.unkword1D977A2?
                    battleCamera.cam.endingCameraRoll = 00000000; //same as above; cam->unkword00A = ff8vars.unkword1D977A2;
                    break;

                case 1:
                    battleCamera.cam.startingCameraRoll = 0;
                    battleCamera.cam.endingCameraRoll = 0;
                    break;

                case 2:
                    current_position = br_.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.startingCameraRoll = current_position;
                    battleCamera.cam.endingCameraRoll = current_position;
                    break;

                case 3:
                    current_position = br_.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.startingCameraRoll = current_position;
                    current_position = br_.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.endingCameraRoll = current_position;
                    break;
            }

            switch (battleCamera.cam.control_word & 1)
            {
                case 0:
                    if (current_position >= 0)
                    {
                        while (true) //I'm setting this to true and breaking in code as this works on peeking on next variable via pointer and that's not possible here without unsafe block
                        {
                            battleCamera.cam.startFramesOffsets[keyframecount] = totalframecount; //looks like this is the camera index
                            current_position = br_.ReadUInt16();
                            if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                break;
                            totalframecount += (ushort)(current_position * 16); //here is increment of short*, but I already did that above
                            battleCamera.cam.is_FrameDurationsShot[keyframecount] = (byte)(current_position = br_.ReadUInt16()); //cam->unkbyte124[keyframecount] = *current_position++; - looks like we are wasting one byte due to integer sizes
                            battleCamera.cam.Camera_World_X_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
                            battleCamera.cam.Camera_World_Y_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
                            battleCamera.cam.Camera_World_Z_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
                            battleCamera.cam.is_FrameEndingShots[keyframecount] = (byte)(current_position = br_.ReadUInt16()); //m->unkbyte204[keyframecount] = *current_position++;
                            battleCamera.cam.Camera_Lookat_X_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
                            battleCamera.cam.Camera_Lookat_Y_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
                            battleCamera.cam.Camera_Lookat_Z_s16[keyframecount] = (short)(current_position = br_.ReadUInt16());
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
                            local14 = (short)(ms_.Position + 5 * 2); //current_position + 5; but current_position is WORD, so multiply by two
                            local10 = (short)(ms_.Position + 6 * 2);
                            local2C = (short)(ms_.Position + 7 * 2);
                            local18 = (short)(ms_.Position + 1 * 2);
                            local1C = (short)(ms_.Position + 2 * 2);
                            while (true)
                            {
                                battleCamera.cam.startFramesOffsets[keyframecount] = totalframecount;
                                current_position = br_.ReadUInt16();
                                if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                    break;
                                totalframecount += (ushort)(current_position * 16);
                                //ff8funcs.Sub503AE0(++local18, ++local1C, ++ebx, *(BYTE*)current_position, &cam->unkword064[keyframecount], &cam->unkword0A4[keyframecount], &cam->unkword0E4[keyframecount]);
                                //ff8funcs.Sub503AE0(++local14, ++local10, ++local2C, *(BYTE*)(current_position + 4), &cam->unkword144[keyframecount], &cam->unkword184[keyframecount], &cam->unkword1C4[keyframecount]);
                                battleCamera.cam.is_FrameEndingShots[keyframecount] = 0xFB;
                                battleCamera.cam.is_FrameDurationsShot[keyframecount] = 0xFB;
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

            if ((battleCamera.cam.control_word & 0x3E) == 0x1E)
            {
                //ff8funcs.Sub503300();
            }
            battleCamera.cam.keyframeCount = keyframecount;
            battleCamera.cam.time = totalframecount;
            //battleCamera.cam.startingTime = 0;
            battleCamera.cam.CurrentTime = TimeSpan.Zero;
            battleCamera.lastCameraPointer = (uint)(ms_.Position + 2);
            battleCamera.bMultiShotAnimation = br.ReadInt16() != -1;
            return (uint)(ms_.Position);
        }

        #endregion fileParsing
    }
}