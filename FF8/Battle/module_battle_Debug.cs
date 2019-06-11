using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF8
{
    static class Module_battle_debug
    {
        private static uint bs_cameraPointer;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees=90, Yshift;
        private static readonly float camDistance = 10.0f;
        private static Vector3 camPosition, camTarget;
        private static TIM2 textureInterface;
        private static Texture2D[] textures;

        static List<EnemyInstanceInformation> EnemyInstances;
        static List<CharacterInstanceInformation> CharacterInstances;

        //skyRotating floats are hardcoded
        private static readonly ushort[] skyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };
        static float localRotator = 0.0f; //a rotator is a float that holds current axis rotation for sky. May be malformed by skyRotators or TimeCompression magic


        public static BasicEffect effect;
        public static AlphaTestEffect ate;

        private static string battlename = "a0stg000.x";
        private static byte[] stageBuffer;

        private static int battleModule = 0;

        //This should be enum btw
        private const int BATTLEMODULE_INIT = 0; //basic init stuff; renderer; core
        private const int BATTLEMODULE_READDATA = 1; //parses battle stage and all monsters
        private const int BATTLEMODULE_DRAWGEOMETRY = 2; //draw geometry also supports updateCamera
        private const int BATTLEMODULE_ACTIVE = 3;
        private const float FPS = 1000.0f / 15f; //Natively the game we are rewritting works in 15 FPS per second

        /// <summary>
        /// This is helper struct that works along with VertexPosition to provide Clut, texture page and bool to decide if it's quad or triangle
        /// </summary>
        private struct Stage_GeometryInfoSupplier
        {
            public bool bQuad;
            public byte clut;
            public byte texPage;
        }

        private struct EnemyInstanceInformation
        {
            public Debug_battleDat Data;
            /// <summary>
            /// bit position of the enemy in encounter data. Use to pair the information with encounter data
            /// </summary>
            public byte index;
            public bool bIsHidden;
            public bool bIsActive;
            public bool bIsUntargetable;
            public AnimationSystem animationSystem;
        }

        /// <summary>
        /// CharacterInstanceInformation should only be used for battle-exclusive data. Manipulating HP, GFs, junctions and other character-specific
        /// things should happen outside battle, because such information about characters is shared between almost all modules. 
        /// This field contains information about the current status of battle rendering like animation frames/ rendering flags/ effects attached
        /// </summary>
        private struct CharacterInstanceInformation
        {
            public CharacterData Data;
            public int characterId; //0 is Whatever guy
            public bool bIsHidden; //GF sequences, magic...
            public AnimationSystem animationSystem;
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

        private struct CharacterData
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

        public static void ResetState() { battleModule = BATTLEMODULE_INIT; }

        public static void Update()
        {
            switch (battleModule)
            {
                case BATTLEMODULE_INIT:
                    InitBattle();
                    break;
                case BATTLEMODULE_READDATA:
                    ReadData();
                    break;
                case BATTLEMODULE_DRAWGEOMETRY:
                    //UpdateCamera();
                    FPSCamera();
                    break;
            }
#if DEBUG
            if (Input.Button(Keys.D1))
                DEBUGframe += 1;
            if (Input.Button(Keys.D2))
                DEBUGframe--;
            if (Input.Button(Keys.D3))
                battleModule = BATTLEMODULE_INIT;
            if(Input.Button(Keys.D4))
            {
                battleModule = BATTLEMODULE_INIT;
                Memory.battle_encounter++;
            }
#endif
        }


        public static void Draw()
        {
            switch (battleModule)
            {
                case BATTLEMODULE_DRAWGEOMETRY:
                    DrawGeometry();
                    DrawMonsters();
                    DrawCharactersWeapons();
                    UpdateFrames();
                    break;

            }
        }
        private static void UpdateCamera()
        {
            const float V = 100f;
            //battleCamera.cam.startingTime = 64;
            float step = battleCamera.cam.startingTime / (float)battleCamera.cam.time;
            float camWorldX = MathHelper.Lerp(battleCamera.cam.Camera_World_X_s16[0] / V,
                battleCamera.cam.Camera_World_X_s16[1] / V, step) -60;
            float camWorldY = MathHelper.Lerp(battleCamera.cam.Camera_World_Y_s16[0] / V,
                battleCamera.cam.Camera_World_Y_s16[1] / V, step); 
            float camWorldZ = MathHelper.Lerp(battleCamera.cam.Camera_World_Z_s16[0] / V,
                battleCamera.cam.Camera_World_Z_s16[1] / V, step) +40;

            float camTargetX = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_X_s16[0] / V,
    battleCamera.cam.Camera_Lookat_X_s16[1] / V, step) -20;
            float camTargetY = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_Y_s16[0] / V,
battleCamera.cam.Camera_Lookat_Y_s16[1] / V, step) +25;
            float camTargetZ = MathHelper.Lerp(battleCamera.cam.Camera_Lookat_Z_s16[0] / V,
battleCamera.cam.Camera_Lookat_Z_s16[1] / V, step) -20;



            camPosition = new Vector3(-camWorldX, camWorldY, -camWorldZ);
            camTarget = new Vector3(camTargetY, -camTargetX, -camTargetZ);


            float fovDirector = MathHelper.Lerp(battleCamera.cam.startingFOV, battleCamera.cam.endingFOV, step);
            
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                   MathHelper.ToRadians(fovDirector/6),
                   Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
    1f, 1000f);

            //ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            //{
            //    Projection = projectionMatrix,
            //    View = viewMatrix,
            //    World = worldMatrix
            //};

            if (battleCamera.cam.startingTime >= battleCamera.cam.time)
                return;
            battleCamera.cam.startingTime += 4;
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
            for (int n = 0; n < CharacterInstances.Count; n++)
            {
                CheckAnimationFrame(Debug_battleDat.EntityType.Character, n);
                Vector3 charaPosition = new Vector3(-40 + n * 10, 0, -40);
                for (int i = 0; i < CharacterInstances[n].Data.character.geometry.cObjects; i++)
                {
                    var a = CharacterInstances[n].Data.character.GetVertexPositions(i,charaPosition ,Quaternion.CreateFromYawPitchRoll(3f,0,0), CharacterInstances[n].animationSystem.animationId, CharacterInstances[n].animationSystem.animationFrame, frameperFPS / FPS); //DEBUG
                    if (a == null || a.Item1.Length == 0)
                        return;
                    for (int k = 0; k < a.Item1.Length / 3; k++)
                    {
                        ate.Texture = CharacterInstances[n].Data.character.textures.textures[a.Item2[k]];
                        foreach (var pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                            vertexData: a.Item1, vertexOffset: k*3, primitiveCount: 1);
                        }
                    }
                }
                DrawShadow(charaPosition, ate, .5f);
            }
            //WEAPON
            for (int n = 0; n < CharacterInstances.Count; n++)
            {   if (CharacterInstances[n].Data.weapon == null)
                    return;
                CheckAnimationFrame(Debug_battleDat.EntityType.Weapon, n);
                Vector3 weaponPosition = new Vector3(-40 + n * 10, 0+DEBUGframe, -40+1);
                for (int i = 0; i < CharacterInstances[n].Data.weapon.geometry.cObjects; i++)
                {
                    var a = CharacterInstances[n].Data.weapon.GetVertexPositions(i, weaponPosition, Quaternion.CreateFromYawPitchRoll(3f, 0, 0), CharacterInstances[n].animationSystem.animationId, CharacterInstances[n].animationSystem.animationFrame, frameperFPS / FPS); //DEBUG
                    if (a == null || a.Item1.Length == 0)
                        return;
                    for (int k = 0; k < a.Item1.Length / 3; k++)
                    {
                        ate.Texture = CharacterInstances[n].Data.weapon.textures.textures[a.Item2[k]];
                        foreach (var pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                            vertexData: a.Item1, vertexOffset: k*3, primitiveCount: 1);
                        }
                    }
                }
            }
        }

        private static void CheckAnimationFrame(Debug_battleDat.EntityType type, int n)
        {
            Debug_battleDat.Animation animationSystem;
            switch(type)
            {
                case Debug_battleDat.EntityType.Monster:
                    animationSystem = EnemyInstances[n].Data.animHeader.animations[EnemyInstances[n].animationSystem.animationId];
                    if (EnemyInstances[n].animationSystem.animationFrame >= animationSystem.cFrames)
                    {
                        var InstanceInformationProvider = EnemyInstances[n];
                        InstanceInformationProvider.animationSystem.animationFrame = 0;
                        if (EnemyInstances[n].animationSystem.AnimationQueue.Count > 0)
                        {
                            InstanceInformationProvider.animationSystem.animationId = EnemyInstances[n].animationSystem.AnimationQueue.First();
                            EnemyInstances[n].animationSystem.AnimationQueue.RemoveAt(0);
                        }
                        EnemyInstances[n] = InstanceInformationProvider;
                    }
                    return;
                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    animationSystem = CharacterInstances[n].Data.character.animHeader.animations[CharacterInstances[n].animationSystem.animationId];
                    if (CharacterInstances[n].animationSystem.animationFrame >= animationSystem.cFrames)
                    {
                        var InstanceInformationProvider = CharacterInstances[n];
                        InstanceInformationProvider.animationSystem.animationFrame = 0;
                        if (CharacterInstances[n].animationSystem.AnimationQueue.Count > 0)
                        {
                            InstanceInformationProvider.animationSystem.animationId = CharacterInstances[n].animationSystem.AnimationQueue.First();
                            CharacterInstances[n].animationSystem.AnimationQueue.RemoveAt(0);
                        }
                        CharacterInstances[n] = InstanceInformationProvider;
                    }
                    return;
                default:
                    return;
            }
        }



        /// <summary>
        /// Animation system. Decided to go for struct, so I can attach it to instance and manipulate easily grouped. It's also open for modifications
        /// </summary>
        private struct AnimationSystem
        {
            public int animationId;
            public int animationFrame;
            public bool bStopAnimation; //pertification placeholder? 
            public List<int> AnimationQueue;
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

            if (EnemyInstances == null)
                return;
            for (int n = 0; n < EnemyInstances.Count; n++)
            {
                if (EnemyInstances[n].Data.GetId == 127)
                {
                    //TODO;
                    continue;
                }

                CheckAnimationFrame(Debug_battleDat.EntityType.Monster, n);

                var enemyPosition = Memory.encounters[Memory.battle_encounter].enemyCoordinates.GetEnemyCoordinateByIndex(EnemyInstances[n].index);

                for (int i = 0; i < EnemyInstances[n].Data.geometry.cObjects; i++)
                {
                    var a = EnemyInstances[n].Data.GetVertexPositions(
                        objectId: i,
                        position: enemyPosition.GetVector(),
                        rotation: Quaternion.CreateFromYawPitchRoll(0, 0, 0),
                        animationId: EnemyInstances[n].animationSystem.animationId,
                        animationFrame: EnemyInstances[n].animationSystem.animationFrame,
                        step: frameperFPS / FPS);
                    if (a == null || a.Item1.Length == 0)
                        return;
                    for (int k = 0; k < a.Item1.Length / 3; k++)
                    {
                        ate.Texture = EnemyInstances[n].Data.textures.textures[a.Item2[k]];
                        foreach (var pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                            vertexData: a.Item1, vertexOffset: k * 3, primitiveCount: 1);
                        }
                    }
                }
                DrawShadow(enemyPosition.GetVector(), ate, EnemyInstances[n].Data.skeleton.GetScale.X/5);
            }
        }

        private static void DrawShadow(Vector3 enemyPosition, AlphaTestEffect ate, float scale)
        {
            VertexPositionTexture[] ptCopy = Memory.shadowGeometry.Clone() as VertexPositionTexture[];
            for(int i = 0; i<ptCopy.Length; i++)
                ptCopy[i].Position =  Vector3.Transform(ptCopy[i].Position, Matrix.CreateScale(scale));
            for (int i = 0; i < ptCopy.Length; i++)
                ptCopy[i].Position = Vector3.Add(ptCopy[i].Position, new Vector3(enemyPosition.X, 0.1f, enemyPosition.Z));
            ate.Texture = Memory.shadowTexture;
            foreach(var pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ptCopy, 0, 8);
            }
        }

        /// <summary>
        /// Increments animation frames by N, where N is equal to int(deltaTime/FPS).
        /// 15FPS is one frame per ~66 miliseconds. Therefore if deltaTime hits at least:
        /// below 33, then frame gets interpolated
        /// above 122, then frame gets skipped (by x/66)
        /// </summary>
        private static void UpdateFrames()
        {
            float tick = Memory.gameTime.ElapsedGameTime.Milliseconds;
            frameperFPS += tick;
            if (frameperFPS > FPS)
            {
                for (int x = 0; x < EnemyInstances.Count; x++)
                {
                    var InstanceInformationProvider = EnemyInstances[x];
                    InstanceInformationProvider.animationSystem.animationFrame++;
                    EnemyInstances[x] = InstanceInformationProvider;
                }
                for (int x = 0; x < CharacterInstances.Count; x++)
                {
                    var InstanceInformationProvider = CharacterInstances[x];
                    InstanceInformationProvider.animationSystem.animationFrame++;
                    CharacterInstances[x] = InstanceInformationProvider;
                }
                frameperFPS = 0.0f;
            }
        }

        /// <summary>
        /// Plays requested animation for given entity immidiately (without waiting for current animation to stop if have any queued animations)
        /// </summary>
        /// <param name="entityType">Provide either Monster or Character/weapon</param>
        /// <param name="nIndex">Index of entityTypeInstance. Monster is monsterInstances, character is CharacterInstances</param>
        /// <param name="newAnimId">self explanatory</param>
        public static void PlayAnimationImmidiately(Debug_battleDat.EntityType entityType, int nIndex, int newAnimId)
        {

            switch(entityType)
            {
                case Debug_battleDat.EntityType.Monster:
                    EnemyInstanceInformation MInstanceInformationProvider = EnemyInstances[nIndex];
                    MInstanceInformationProvider.animationSystem.animationId = newAnimId;
                    MInstanceInformationProvider.animationSystem.animationFrame = 0;
                    EnemyInstances[nIndex] = MInstanceInformationProvider;
                    return;
                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    CharacterInstanceInformation CInstanceInformationProvider = CharacterInstances[nIndex];
                    CInstanceInformationProvider.animationSystem.animationId = newAnimId;
                    CInstanceInformationProvider.animationSystem.animationFrame = 0;
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
                    EnemyInstanceInformation MInstanceInformationProvider = EnemyInstances[nIndex];
                    MInstanceInformationProvider.animationSystem.AnimationQueue.Add(newAnimId);
                    EnemyInstances[nIndex] = MInstanceInformationProvider;
                    return;
                case Debug_battleDat.EntityType.Character:
                case Debug_battleDat.EntityType.Weapon:
                    CharacterInstanceInformation CInstanceInformationProvider = CharacterInstances[nIndex];
                    CInstanceInformationProvider.animationSystem.AnimationQueue.Add(newAnimId);
                    CharacterInstances[nIndex] = CInstanceInformationProvider;
                    return;
                default:
                    return;
            }
        }

        const float defaultmaxMoveSpeed = 1f;
        const float MoveSpeedChange = 1f;
        static float maxMoveSpeed = defaultmaxMoveSpeed;
        const float maxLookSpeed = 0.25f;
        public static void FPSCamera()
        {
            #region FPScamera
            //speedcontrols
            //+ to increase
            //- to decrease
            //* to reset            
            if (Input.Button(Keys.OemPlus) || Input.Button(Keys.Add))
            {
                maxMoveSpeed += MoveSpeedChange;
            }
            if (Input.Button(Keys.OemMinus) || Input.Button(Keys.Subtract))
            {
                maxMoveSpeed -= MoveSpeedChange;
                if (maxMoveSpeed < defaultmaxMoveSpeed) maxMoveSpeed = defaultmaxMoveSpeed;
            }
            if (Input.Button(Keys.Multiply)) maxMoveSpeed = defaultmaxMoveSpeed;

            //speed is effected by the milliseconds between frames. so alittle goes a long way. :P
            float x_shift = Input.Distance(Buttons.MouseXjoy, maxLookSpeed);
            float y_shift = Input.Distance(Buttons.MouseYjoy, maxLookSpeed);
            float leftdistX = Math.Abs(Input.Distance(Buttons.LeftStickX, maxMoveSpeed));
            float leftdistY = Math.Abs(Input.Distance(Buttons.LeftStickY, maxMoveSpeed));
            x_shift += Input.Distance(Buttons.RightStickX, maxLookSpeed);
            y_shift += Input.Distance(Buttons.RightStickY, maxLookSpeed);
            Yshift -= y_shift;
            degrees = (degrees + (int)x_shift) % 360;
            Yshift = MathHelper.Clamp(Yshift, -80, 80);
            if (leftdistY == 0)
            {
                leftdistY = Input.Distance(maxMoveSpeed);
            }
            if (leftdistX == 0)
            {
                leftdistX = Input.Distance(maxMoveSpeed);
            }

            if (Input.Button(Buttons.Up))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y -= Yshift / 50;
            }
            if (Input.Button(Buttons.Down))
            {
                camPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y += Yshift / 50;
            }
            if (Input.Button(Buttons.Left))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
            }
            if (Input.Button(Buttons.Right))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
            }

            //Input.LockMouse();

            camTarget.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            #endregion
        }
        private static void DrawGeometry()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);

            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;




            effect.TextureEnabled = true;
            for(int n = 0; n < modelGroups.Length; n++)
                foreach (var b in modelGroups[n].models)
                {
                    var vpt = GetVertexBuffer(b);
                    if (n == 3 && skyRotators[Memory.encounters[Memory.battle_encounter].Scenario] != 0)
                        CreateRotation(vpt);
                    if (vpt == null) continue;
                    int localVertexIndex = 0;
                    for (int i = 0; i < vpt.Item1.Length; i++)
                    {
                        ate.Texture = textures[vpt.Item1[i].clut]; //provide texture per-face
                        foreach (var pass in ate.CurrentTechnique.Passes)
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
            /*Memory.font.RenderBasicText(
                $"Encounter ready at: {Memory.battle_encounter}\n" +
                $"Debug variable: {DEBUGframe}\n" +
                $"1000/deltaTime milliseconds: {Math.Round((double)1000 / Memory.gameTime.ElapsedGameTime.Milliseconds,2)}",
                30,20,lineSpacing: 5);*/
            Memory.font.RenderBasicText(new FF8String($"Encounter ready at: {Memory.battle_encounter}"), 0, 0, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Debug variable: {DEBUGframe}"), 20, 30 * 1, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"1000/deltaTime milliseconds: {1000/Memory.gameTime.ElapsedGameTime.Milliseconds}"), 20, 30 * 2, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"camera frame: {battleCamera.cam.startingTime}/{battleCamera.cam.time}"), 20, 30 * 3, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Position: {Extended.RemoveBrackets(camPosition.ToString())}"), 20, 30 * 4, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Target: {Extended.RemoveBrackets(camTarget.ToString())}"), 20, 30 * 5, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.FOV: {MathHelper.Lerp(battleCamera.cam.startingFOV, battleCamera.cam.endingFOV, battleCamera.cam.startingTime / (float)battleCamera.cam.time)}"), 20, 30 * 6, 1, 1, 0, 1);

            Memory.SpriteBatchEnd();
        }

        private static void CreateRotation(Tuple<Stage_GeometryInfoSupplier[], VertexPositionTexture[]> vpt)
        {
            localRotator += (short)skyRotators[Memory.encounters[Memory.battle_encounter].Scenario]/4096f * Memory.gameTime.ElapsedGameTime.Milliseconds;
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
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.quads[i].U3, model.quads[i].V3, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, textureInterface.GetWidth)));

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

        private static void InitBattle()
        {
            //MakiExtended.Debugger_Spawn();
            //MakiExtended.Debugger_Feed(typeof(Module_battle_debug), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Input.OverrideLockMouse=true;
            Input.CurrentMode = Input.MouseLockMode.Center;

            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            int stage = enc.Scenario;
            battlename = $"a0stg{stage.ToString("000")}.x";
            Console.WriteLine($"BS_DEBUG: Loading stage {battlename}");
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.battle_encounter}\t cEnemies: {enc.EnabledEnemy}\t Enemies: {string.Join(",", enc.BEnemies.Where(x => x != 0x00).Select(x => $"{x}").ToArray())}");


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

        #region fileParsing

        private static void ReadData()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] test = aw.GetListOfFiles();
            battlename = test.First(x => x.ToLower().Contains(battlename));
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

            ReadTexture(MainSection.TexturePointer);
            br.Close();
            ms.Close();
            ms.Dispose();

            ReadCharacters();
            ReadMonster();

            battleModule++;
        }

        public static int DEBUG = 0;
        private static float frameperFPS = 0.0f;

        private static void ReadCharacters()
        {
            CharacterInstances = new List<CharacterInstanceInformation>
            {
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(0,0,0),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new List<int>()},
                    characterId = 0,
                },
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(1,3,8),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new List<int>()},
                    characterId = 1
                },
                new CharacterInstanceInformation()
                {
                    Data = ReadCharacterData(2,6,13),
                    animationSystem = new AnimationSystem(){ AnimationQueue = new List<int>()},
                    characterId = 2
                }
            };
        }

        private static CharacterData ReadCharacterData(int characterId, int alternativeCostumeId, int weaponId)
        {
            var character = new Debug_battleDat(characterId, Debug_battleDat.EntityType.Character, alternativeCostumeId);
            Debug_battleDat weapon;
            if (characterId == 1 || characterId == 9)
                weapon = new Debug_battleDat(characterId, Debug_battleDat.EntityType.Weapon, weaponId, character);
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (weaponId != -1) weapon = new Debug_battleDat(characterId, Debug_battleDat.EntityType.Weapon, weaponId);
#pragma warning restore IDE0045 // Convert to conditional expression
            else weapon = null;
            return new CharacterData()
            {
                character = character,
                weapon = weapon
            };
        }

        /// <summary>
        /// This method is responsible to read/parse the enemy data. It holds the result in monstersData[]
        /// This method was designed to read only one instance of enemy. A list called EnemyInstance holds data information for each enemy
        /// </summary>
        private static void ReadMonster()
        {
            //for(int i = 0; i<199; i++)
            //{
            //    var dcd = new Debug_battleDat(i, Debug_battleDat.EntityType.Monster);
            //    var ecd = dcd.skeleton.GetScale.Y;
            //    Console.WriteLine($"{i}/{ecd}");
            //}
            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            if (enc.EnabledEnemy == 0)
            {
                monstersData = new Debug_battleDat[0];
                return;
            }
            var DistinctMonsterPointers = enc.BEnemies.GroupBy(x => x).ToArray();
            monstersData = new Debug_battleDat[DistinctMonsterPointers.Count()];
            for (int n = 0; n < monstersData.Length; n++)
                monstersData[n] = new Debug_battleDat(DistinctMonsterPointers[n].Key, Debug_battleDat.EntityType.Monster);
            if (monstersData == null)
                monstersData = new Debug_battleDat[0];
            EnemyInstances = new List<EnemyInstanceInformation>();
            for(int i = 0; i<8; i++)
                if (Extended.GetBit(enc.EnabledEnemy, 7-i))
                    EnemyInstances.Add(new EnemyInstanceInformation() { Data = monstersData.Where(x => x.GetId == enc.BEnemies[i] ).First(),
                        bIsHidden =Extended.GetBit(enc.HiddenEnemies, 7-i),
                        bIsActive = true,
                        index = (byte)(7-i),
                        bIsUntargetable = Extended.GetBit(enc.UntargetableEnemy, 7-i),
                        animationSystem = new AnimationSystem() { AnimationQueue= new List<int>()}}
                        );
            
        }

        /// <summary>
        /// Method designed for Stage texture loading. 
        /// </summary>
        /// <param name="texturePointer">Absolute pointer to TIM texture header in stageBuffer</param>
        private static void ReadTexture(uint texturePointer)
        {
            textureInterface = new TIM2(stageBuffer, texturePointer);
            textures = new Texture2D[textureInterface.GetClutCount];
            for (ushort i = 0; i < textureInterface.GetClutCount; i++)
            {
                byte[] b = textureInterface.CreateImageBuffer(textureInterface.GetClutColors(i));
                Texture2D tex = new Texture2D(Memory.spriteBatch.GraphicsDevice,
                    textureInterface.GetWidth, textureInterface.GetHeight, false, SurfaceFormat.Color);
                tex.SetData(b);
                textures[i] = tex;
            }
        }

        /// <summary>
        /// Reads Stage model groups pointers and reads/parses them individually. 
        /// Group0 is stage ground. It's always enabled except special sequences like GFs
        /// Group1 is main geometry. It's prior to Time Compression deformation
        /// Group2 is main/additional geometry. It's prior to Time Compression deformation
        /// Group3 is Sky. It's NON-prior to Time Compression, but may be modified by SkyRotators and/or TimeCompression last Stage skyRotation multiplier
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
        /// This is the main class that reads given Stage geometry group. It stores the data into Model structure
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
        /// Gets vanilla engine camera pointers. A team that rewrote the game into PC just left PlayStation MIPS data inside files
        /// and therefore their code is to skip given hardcoded data which in fact are PS compiled instructions
        /// This data is naturally read by PlayStation in original console release.
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

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =1092)]
        public struct CameraStruct
        {
            public byte unkbyte000; //000
            public byte keyframeCount;
            public ushort control_word;
            public ushort startingFOV; //usually ~280
            public ushort endingFOV; //006
            public ushort startingCameraRoll; //usually 0 unless you're aiming for some wicked animation
            public ushort endingCameraRoll; //
            public ushort startingTime; //usually 0, that's pretty logical
            /// <summary>
            /// Time is calculated from number of frames. You basically set starting position World+lookat and ending position, then mark number of frames to interpolate between them. Every frame is one drawcall and it costs 16.
            /// </summary>
            public ushort time; //starting time needs to be equal or higher for next animation frame to be read; If next frame==0xFFFF then it's all done
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =20)]
            public byte[] unk; //010

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public ushort[] unkword024; //024 - start frames for each key frame?
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Z_s16; //064
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_X_s16; //0A4
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_World_Y_s16; //0E4
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] unkbyte124; //124
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Z_s16; //144
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_X_s16; //184
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public short[] Camera_Lookat_Y_s16; //1C4
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] unkbyte204; //204
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
        };

        /// <summary>
        /// Main battle camera struct. It holds data for camera settings, camera collections and main CameraStruct used as a final result of camera parsing
        /// </summary>
        private struct BattleCamera
        {
            public BattleCameraSettings battleCameraSettings;
            public BattleCameraCollection battleCameraCollection;
            public CameraStruct cam;
        }

        private static BattleCamera battleCamera;

        /// <summary>
        /// Battle camera settings are about 32 bytes of unknown flags and variables used in whole stage including geometry
        /// </summary>
        private struct BattleCameraSettings
        {
            public byte[] unk;
        }

        /// <summary>
        /// Main struct for collection of camera animations. Every BattleCameraSet hold 8 animations no matter what
        /// </summary>
        private struct BattleCameraCollection
        {
            public uint cAnimCollectionCount;
            public uint pCameraEOF;
            public BattleCameraSet[] battleCameraSet;
        }

        /// <summary>
        /// Struct for battle camera animation set. Animation set always contain 8 animations.
        /// This struct does not contain a data for pre-readed information. Therefore you have to call ReadAnimation(index)
        /// to actually read the animation to BattleCamera.cam(CameraStruct). That is because there are extreme amount
        /// of cases where the camera is changing and reading again and again not including the battle stage.
        /// Also reading all camera animations is waste of time and resources
        /// </summary>
        private struct BattleCameraSet
        {
            public uint[] animPointers;
            public uint globalSetPointer;
        }

        /// <summary>
        /// Parses camera data into BattleCamera struct. Main purpouse of this function is to actually read all the offsets and pointers to human readable form of struct.
        /// This function later calls ReadAnimation(n) where n is animation Id (i.e. 9 is camCollection=1 and cameraAnim=0)
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

            ReadAnimation(GetRandomCameraN(Memory.encounters[Memory.battle_encounter]));

            ms.Seek(bs_cameraPointer + sCameraDataSize, 0); //step out
        }

        /// <summary>
        /// Gets random camera from available from encounter- primary or secondary
        /// </summary>
        /// <param name="encounter">instance of current encounter</param>
        /// <returns>Either primary or alternative camera from encounter</returns>
        private static int GetRandomCameraN(Init_debugger_battle.Encounter encounter)
        {
            int camToss = Memory.random.Next(0, 3) < 2 ? 0 : 1; //primary camera has 2/3 chance of beign selected
            switch(camToss)
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
        /// Returns tuple containing camera animation set pointer and camera animation in that set
        /// </summary>
        /// <param name="animId">6bit variable containing camera pointer</param>
        /// <returns>Tuple with CameraSetPointer, CameraSetPointer[CameraAnimationPointer]</returns>
        private static Tuple<int, int> GetCameraCollectionPointers(int animId)
        {
            var enc = Memory.encounters[Memory.battle_encounter];
            int pSet = enc.ResolveCameraSet((byte)animId);
            int pAnim = enc.ResolveCameraAnimation((byte)animId);
            return new Tuple<int, int>(pSet, pAnim);
        }


        /// <summary>
        /// Method that provides reading of real animation data based on 6bit camera pointer. In future do overload on this function
        /// to support GF and magic reading
        /// </summary>
        /// <param name="animId"></param>
        private static uint ReadAnimation(int animId)
        {
            short local2C;
            byte keyframecount =0;
            ushort totalframecount = 0;
            short local1C;
            short local18;
            short local14;
            short local10;


            var tpGetter = GetCameraCollectionPointers(animId);
            uint cameraAnimationGlobalPointer = battleCamera.battleCameraCollection.battleCameraSet[tpGetter.Item1].animPointers[tpGetter.Item2];
            ms.Seek(cameraAnimationGlobalPointer, SeekOrigin.Begin);
            battleCamera.cam.control_word = br.ReadUInt16(); 
            if (battleCamera.cam.control_word == 0xFFFF)
                return 0; //return NULL

            var current_position = br.ReadUInt16(); //getter for *current_position
            ms.Seek(-2, SeekOrigin.Current); //roll back one WORD because no increment

            switch((battleCamera.cam.control_word >> 6) & 3)
            {
                case 1:
                    battleCamera.cam.startingFOV = 0x200;
                    battleCamera.cam.endingFOV = 0x200;
                    break;
                case 2:
                    battleCamera.cam.startingFOV = current_position;
                    battleCamera.cam.endingFOV = current_position;
                    br.ReadUInt16(); //current_position++
                    break;
                case 3:
                    battleCamera.cam.startingFOV = current_position;
                    current_position = br.ReadUInt16();
                    battleCamera.cam.endingFOV = current_position;
                    ms.Seek(2, SeekOrigin.Current); //skipping WORD, because we already rolled back one WORD above this switch
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
                    current_position = br.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.startingCameraRoll = current_position;
                    battleCamera.cam.endingCameraRoll = current_position;
                    break;
                case 3:
                    current_position = br.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.startingCameraRoll = current_position;
                    current_position = br.ReadUInt16(); //* + current_position++;
                    battleCamera.cam.endingCameraRoll = current_position;
                    break;
            }

            switch (battleCamera.cam.control_word & 1)
            {
                case 0:
                    if(current_position >= 0)
                    {
                        while(true) //I'm setting this to true and breaking in code as this works on peeking on next variable via pointer and that's not possible here without unsafe block
                        {
                            battleCamera.cam.unkword024[keyframecount] = totalframecount;
                            current_position = br.ReadUInt16();
                            if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                break;
                            totalframecount += (ushort)(current_position * 16); //here is increment of short*, but I already did that above
                            battleCamera.cam.unkbyte124[keyframecount] = (byte)(current_position =  br.ReadUInt16()); //cam->unkbyte124[keyframecount] = *current_position++; - looks like we are wasting one byte due to integer sizes
                            battleCamera.cam.Camera_World_Z_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            battleCamera.cam.Camera_World_X_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            battleCamera.cam.Camera_World_Y_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            battleCamera.cam.unkbyte204[keyframecount] = (byte)(current_position = br.ReadUInt16()); //m->unkbyte204[keyframecount] = *current_position++;
                            battleCamera.cam.Camera_Lookat_Z_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            battleCamera.cam.Camera_Lookat_X_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            battleCamera.cam.Camera_Lookat_Y_s16[keyframecount] = (short)(current_position = br.ReadUInt16());
                            keyframecount++;
                        }

                        if(keyframecount>2)
                        {
                            //ff8funcs.Sub50D010(cam->unkword024, cam->unkword064, cam->unkword0A4, cam->unkword0E4, keyframecount, cam->unkbyte224, cam->unkbyte2A4, cam->unkbyte324);
                            //ff8funcs.Sub50D010(cam->unkword024, cam->unkword144, cam->unkword184, cam->unkword1C4, keyframecount, cam->unkbyte3A4, cam->unkbyte424, cam->unkbyte4A4);
                        }
                    }
                    break;
                case 1:
                    {
                        if(current_position >= 0)
                        {
                            local14 = (short)(ms.Position + 5*2); //current_position + 5; but current_position is WORD, so multiply by two
                            local10 = (short)(ms.Position + 6*2);
                            local2C = (short)(ms.Position + 7*2);
                            local18 = (short)(ms.Position + 1*2);
                            local1C = (short)(ms.Position + 2*2);
                            while(true)
                            {
                                battleCamera.cam.unkword024[keyframecount] = totalframecount;
                                current_position = br.ReadUInt16();
                                if ((short)current_position < 0) //reverse of *current_position >= 0, also cast to signed is important here
                                    break;
                                totalframecount += (ushort)(current_position * 16);
					//ff8funcs.Sub503AE0(++local18, ++local1C, ++ebx, *(BYTE*)current_position, &cam->unkword064[keyframecount], &cam->unkword0A4[keyframecount], &cam->unkword0E4[keyframecount]);
					//ff8funcs.Sub503AE0(++local14, ++local10, ++local2C, *(BYTE*)(current_position + 4), &cam->unkword144[keyframecount], &cam->unkword184[keyframecount], &cam->unkword1C4[keyframecount]);
					battleCamera.cam.unkbyte204[keyframecount] = 0xFB;
					battleCamera.cam.unkbyte124[keyframecount] = 0xFB;
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

            if((battleCamera.cam.control_word & 0x3E) == 0x1E)
            {
                //ff8funcs.Sub503300();
            }
            battleCamera.cam.keyframeCount = keyframecount;
            battleCamera.cam.time = totalframecount;
            battleCamera.cam.startingTime = 0;
            return (uint)(ms.Position+2);
        }


        #endregion
    }
}
