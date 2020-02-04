using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Battle;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII
{
    public static partial class Module_battle_debug
    {
        #region Fields

        public const int Yoffset = 0;
        public static AlphaTestEffect ate;
        public static int DEBUG = 0;
        public static bool PauseATB = false;

        /// <summary>
        /// controls the amount of battlecamera.time incrementation- lower value means longer camera animation
        /// </summary>
        private const int BATTLECAMERA_FRAMETIME = 3;

        private const int BATTLEMODULE_ACTIVE = 3;
        private const int BATTLEMODULE_DRAWGEOMETRY = 2;

        //This should be enum btw
        private const int BATTLEMODULE_INIT = 0;

        private const int BATTLEMODULE_READDATA = 1;

        //parses battle stage and all monsters
        //draw geometry also supports updateCamera
        private static readonly TimeSpan FPS = TimeSpan.FromMilliseconds(1000.0d / 15d);

        private static BinaryReader br;
        private static uint bs_cameraPointer;
        private static bool bUseFPSCamera = false;
        private static Vector3 camPosition, camTarget;

        //private static List<EnemyInstanceInformation> EnemyInstances;
        private static List<CharacterInstanceInformation> CharacterInstances;

        private static DeadTime DeadTime;
        private static float degrees = 90;
        private static BasicEffect effect;
        private static bool ForceReload = false;
        private static FPS_Camera fps_camera;
        private static TimeSpan FrameTime = TimeSpan.Zero;
        private static List<Battle.Mag> MagALL;
        private static Debug_battleDat[] monstersData;
        private static MemoryStream ms;
        private static sbyte? partypos = null;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static Vector3 PyramidOffset = new Vector3(0, 3f, 0);
        private static Battle.RegularPyramid RegularPyramid;
        private static ConcurrentDictionary<Characters, List<byte>> s_weapons;
        private static byte SID = 0;
        private static byte[] stageBuffer;

        #endregion Fields

        #region Enums

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

        #endregion Enums

        #region Properties

        public static int battleModule { get; set; } = 0;
        public static Camera Camera { get; private set; }
        public static Vector3 CamPosition { get => camPosition; private set => camPosition = value; }

        //Natively the game we are rewritting works in 15 FPS per second
        //-10;
        public static Vector3 CamTarget { get => camTarget; private set => camTarget = value; }

        //basic init stuff; renderer; core
        public static ConcurrentDictionary<Characters, SortedSet<byte>> Costumes { get; private set; }

        public static int DEBUGframe { get; private set; } = 0;
        public static BasicEffect Effect { get => effect; private set => effect = value; }
        public static Matrix ProjectionMatrix { get => projectionMatrix; private set => projectionMatrix = value; }
        public static Battle.Stage Stage { get; set; }
        public static Matrix ViewMatrix { get => viewMatrix; private set => viewMatrix = value; }

        public static ConcurrentDictionary<Characters, List<byte>> Weapons
        {
            get
            {
                FillWeapons();
                return s_weapons;
            }
            private set => s_weapons = value;
        }

        public static Matrix WorldMatrix { get => worldMatrix; private set => worldMatrix = value; }

        private static Vector3 CenterOfScreen => Memory.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(Memory.graphics.GraphicsDevice.Viewport.Width / 2f, Memory.graphics.GraphicsDevice.Viewport.Height / 2f, 0f), ProjectionMatrix, ViewMatrix, WorldMatrix);

        private static IGMDataItem.Icon CROSSHAIR { get; set; }

        private static IEnumerable<Battle.Mag> MagGeometries => MagALL?.Where(x => (x.Geometries?.Count ?? 0) > 0) ?? null;

        private static IEnumerable<Battle.Mag> MagPacked => MagALL?.Where(x => x.isPackedMag) ?? null;

        private static IEnumerable<Battle.Mag> MagTIMs => MagALL?.Where(x => x.isTIM) ?? null;

        private static IEnumerable<int> MagUNKID => MagALL?.Where(x => x.UnknownType > 0).Select(x => x.UnknownType) ?? null;

        #endregion Properties

        #region Methods

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

        public static Matrix CreateBillboard(Vector3 pos) => Matrix.CreateBillboard(pos, CenterOfScreen, Vector3.Up, null);

        public static Matrix CreateConstrainedBillboard(Vector3 pos, Vector3 rotateAxis) => Matrix.CreateConstrainedBillboard(pos, CenterOfScreen, rotateAxis, null, null);

        public static void Draw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            switch (battleModule)
            {
                case BATTLEMODULE_DRAWGEOMETRY:
                    //DrawGeometry();
                    DrawMonsters();
                    DrawCharactersWeapons();
                    RegularPyramid.Draw(worldMatrix, viewMatrix, projectionMatrix);
                    Stage.Draw();
                    Vector3 v = GetIndicatorPoint(-1);
                    v.Y -= 5f;
                    if (!bUseFPSCamera)
                        Menu.BattleMenus.Draw();
                    break;
            }

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(new FF8String($"Encounter ready at: {Memory.Encounters.ID} - {Memory.Encounters.Filename}"), 20, 0, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Debug variable: {DEBUGframe} ({DEBUGframe >> 4},{DEBUGframe & 0b1111})"), 20, 30 * 1, 1, 1, 0, 1);
            if (Memory.gameTime.ElapsedGameTime.TotalMilliseconds > 0)
                Memory.font.RenderBasicText(new FF8String($"1000/deltaTime milliseconds: {1000 / Memory.gameTime.ElapsedGameTime.TotalMilliseconds}"), 20, 30 * 2, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"camera frame: {Camera.cam.CurrentTime}/{Camera.cam.TotalTime}"), 20, 30 * 3, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Position: {Extended.RemoveBrackets(camPosition.ToString())}"), 20, 30 * 4, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.World.Target: {Extended.RemoveBrackets(camTarget.ToString())}"), 20, 30 * 5, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.FOV: {MathHelper.Lerp(Camera.cam.startingFOV, Camera.cam.endingFOV, Camera.cam.CurrentTime.Ticks / (float)Camera.cam.TotalTime.Ticks)}"), 20, 30 * 6, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Camera.Mode: {Camera.cam.control_word & 1}"), 20, 30 * 7, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"DEBUG: Press 0 to switch between FPSCamera/Camera anim: {bUseFPSCamera}"), 20, 30 * 8, 1, 1, 0, 1);
            Memory.font.RenderBasicText(new FF8String($"Sequence ID: {SID}, press F10 to activate sequence, F11 SID--, F12 SID++"), 20, 30 * 9, 1, 1, 0, 1);

            Memory.SpriteBatchEnd();
        }

        public static void DrawCrosshair(Enemy enemy)
        {
            IGMData.Limit.Shot shot = Menu.BattleMenus.GetCurrentBattleMenu()?.Shot;
            if (shot != null && shot.Enabled)
            {
                Damageable[] targets = shot?.Targets;
                if (targets != null)
                    foreach (Damageable d in targets)
                    {
                        if (d.GetEnemy(out Enemy e) && e.Equals(enemy))
                        {
                            Vector3 posIn3DSpace = e.EII.Data.IndicatorPoint(e.EII.Location);
                            posIn3DSpace.Y -= 1f;
                            Vector3 ScreenPos = Memory.graphics.GraphicsDevice.Viewport.Project(posIn3DSpace, ProjectionMatrix, ViewMatrix, WorldMatrix);
                            Memory.SpriteBatchStartAlpha();
                            CROSSHAIR.Pos = new Rectangle(new Vector2(ScreenPos.X, ScreenPos.Y).ToPoint(), Point.Zero);
                            EntryGroup icons = Memory.Icons[CROSSHAIR.Data];
                            TextureHandler texture = Memory.Icons.GetTexture(Icons.ID.Cross_Hair1);
                            Vector2 s = texture.ScaleFactor;
                            CROSSHAIR.Pos.Offset(-icons.Width * s.X / 2f, -icons.Height * s.Y / 2f);
                            CROSSHAIR.Draw();
                            Memory.SpriteBatchEnd();
                            break;
                        }
                    }
            }
        }

        public static Vector3 GetIndicatorPoint(int n)
        {
            if ((CharacterInstances == null && n >= 0) || ((Enemy.Party == null || Enemy.Party.Count == 0) && n < 0))
                return Vector3.Zero;
            else
            {
                EnemyInstanceInformation eII = Enemy.Party.FirstOrDefault(x => x.EII.partypos == n)?.EII;
                return (n >= 0 ? CharacterInstances[n].Data.character.IndicatorPoint(CharacterInstances[n].Data.Location) :
                eII.Data.IndicatorPoint(eII.Location)) + PyramidOffset;
            }
        }

        public static void Inputs()
        {
            if (Input2.Button(Keys.D0))
                bUseFPSCamera = !bUseFPSCamera;
            else if (Input2.Button(Keys.D1))
            {
                if ((DEBUGframe & 0b1111) >= 7)
                {
                    DEBUGframe += 0b00010000;
                    DEBUGframe -= 7;
                }
                else DEBUGframe += 1;
                Camera.ChangeAnimation((byte)DEBUGframe);
            }
            else if (Input2.Button(Keys.D2))
            {
                if ((DEBUGframe & 0b1111) == 0)
                {
                    DEBUGframe -= 0b00010000;
                    DEBUGframe += 7;
                }
                else DEBUGframe--;
                Camera.ChangeAnimation((byte)DEBUGframe);
            }
            else if (Input2.Button(Keys.F5))
            {
                //reload
                battleModule = BATTLEMODULE_INIT;
                ForceReload = true;
                Memory.SuppressDraw = true;
            }
            else if (Input2.Button(Keys.D3))
            {
                battleModule = BATTLEMODULE_INIT;
                Memory.Encounters.Previous();
                Memory.SuppressDraw = true;
            }
            else if (Input2.Button(Keys.D4))
            {
                battleModule = BATTLEMODULE_INIT;
                Memory.Encounters.Next();
                Memory.SuppressDraw = true;
            }
            else if (Input2.Button(Keys.D5))
            {
                AddAnimationToQueue(Debug_battleDat.EntityType.Monster, 0, 3);
                AddAnimationToQueue(Debug_battleDat.EntityType.Monster, 0, 0);
            }
            else if (Input2.Button(Keys.F12))
            {
                if (SID < 255)
                    SID++;
                else SID = 0;
            }
            else if (Input2.Button(Keys.F11))
            {
                if (SID <= 0)
                    SID = 255;
                else SID--;
            }
            else if (Input2.Button(Keys.F10))
            {
                AddSequenceToAllQueues(SID);
            }
            else if (Input2.Button(Keys.F9))
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
            else if (Input2.Button(Keys.F8))
            {
                StopAnimations();
            }
            else if (Input2.Button(Keys.F7))
            {
                StartAnimations();
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

        public static void ResetState() => battleModule = BATTLEMODULE_INIT;

        public static void Update()
        {
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
                }
            if (Enemy.Party != null)
                foreach (Enemy e in Enemy.Party)
                    e.Update(); //updates ATB for enemy.
            bool ret = false;
            switch (battleModule)
            {
                case BATTLEMODULE_INIT:
                    Memory.SuppressDraw = true;
                    InitBattle();
                    break;

                case BATTLEMODULE_READDATA:
                    Memory.SuppressDraw = true;
                    ReadData();
                    Menu.BattleMenus.Refresh();
                    Menu.FadeIn();
                    ForceReload = false;
                    break;

                case BATTLEMODULE_DRAWGEOMETRY:
                    Stage?.Update();
                    DeadTime?.Update();
                    Menu.BattleMenus.Update();
                    sbyte? partypos = Menu.BattleMenus.PartyPos;
                    RegularPyramid.Set(GetIndicatorPoint(partypos ?? 0));
                    if (partypos != Module_battle_debug.partypos)
                    {
                        if (partypos == null)
                        {
                            RegularPyramid.FadeOut();
                        }
                        else
                        {
                            RegularPyramid.FadeIn();
                        }
                        Module_battle_debug.partypos = partypos;
                    }
                    if (bUseFPSCamera)
                    {
                        viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                        projectionMatrix = Camera.projectionMatrix;
                    }
                    else
                    {
                        Camera?.Update();
                        viewMatrix = Camera.viewMatrix;
                        projectionMatrix = Camera.projectionMatrix;
                        ret = Menu.BattleMenus.Inputs();
                    }
                    break;
            }
            if (!ret) Inputs();
            RegularPyramid.Update();
            UpdateFrames();
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

        private static bool CharacterInstanceAnimationStopped(int n) =>
            CharacterInstances[n].animationSystem.AnimationStopped ||
            ((Memory.State?[(Characters)CharacterInstances[n].characterId]?.IsPetrify ?? false) &&
            CharacterInstances[n].animationSystem.StopAnimation());

        private static double CharacterInstanceGenerateStep(int n) => GenerateStep(CharacterInstanceAnimationStopped(n));

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

                    //these are added in remaster as possible items.
                    //const byte Ribbon = 100;
                    //const byte Friendship = 32;
                    //const byte Mogs_Amulet = 65;

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

        private static void DrawBattleDat(Debug_battleDat battledat, double step, ref AnimationSystem animationSystem, ref Vector3 position, Quaternion? _rotation = null)
        {
            for (int i = 0; /*i<1 &&*/ i < battledat.geometry.cObjects; i++)
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
                Vector3 charaPosition = CharacterInstances[n].Data.Location = GetCharPos(n);
                DrawBattleDat(CharacterInstances[n].Data.character, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].animationSystem, ref charaPosition);
                DrawShadow(charaPosition, ate, .5f);

                //WEAPON
                if (CharacterInstances[n].Data.weapon != null)
                {
                    CheckAnimationFrame(Debug_battleDat.EntityType.Weapon, n);
                    DrawBattleDat(CharacterInstances[n].Data.weapon, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].animationSystem, ref charaPosition);
                }
            }
        }

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
                DrawBattleDat(Enemy.Party[n].EII.Data, GenerateStep(EnemyInstanceAnimationStopped(n)), ref Enemy.Party[n].EII.animationSystem, ref enemyPosition, Quaternion.Identity);
                DrawShadow(enemyPosition, ate, Enemy.Party[n].EII.Data.skeleton.GetScale.X / 5);
                DrawCrosshair(Enemy.Party[n]);
            }
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

        private static bool EnemyInstanceAnimationStopped(int n) =>
            Enemy.Party[n].EII.animationSystem.AnimationStopped ||
            (Enemy.Party[n].IsPetrify &&
            Enemy.Party[n].EII.animationSystem.StopAnimation());

        private static void FillCostumes()
        {
            if (Costumes == null)
            {
                Costumes = new ConcurrentDictionary<Characters, SortedSet<byte>>();
                Regex r = new Regex(@"d([\da-fA-F]+)c(\d+)\.dat", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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

        private static void FillWeapons()
        {
            if (s_weapons == null)
            {
                Weapons = new ConcurrentDictionary<Characters, List<byte>>();
                for (int i = 0; i <= (int)Characters.Ward_Zabac; i++)
                {
                    SortedSet<byte> _weapons = new SortedSet<byte>();
                    Regex r = new Regex(@"d(" + i.ToString("X") + @")w(\d+)\.dat", RegexOptions.IgnoreCase | RegexOptions.Compiled);
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

        private static double GenerateStep(bool AnimationStopped)
        {
            if (AnimationStopped)
                return 1d;
            return (double)FrameTime.Ticks / FPS.Ticks;
        }

        private static Vector3 GetCharPos(int _n) => new Vector3(-10 + _n * 10, Yoffset, -30);

        private static byte GetCostume(Characters c) => Memory.State[c].Alternativemodel != 0 ? Costumes[c].First() : Costumes[c].Last();

        private static Vector3 GetEnemyPos(int n)
        {
            //Memory.Encounters.enemyCoordinates[Enemy.Party[n].EII.index];
            //Vector3 a = Memory.Encounters.enemyCoordinates.AverageVector;
            //Vector3 m = Memory.Encounters.enemyCoordinates.MidVector;

            //v.x -= (short)m.X;

            //v.z -= (short)a.Z;
            return Enemy.Party[n].EII.Location;
        }

        private static byte GetWeaponID(Characters c)
        {
            byte weaponId = 0;
            if (Memory.State.Characters.TryGetValue(c, out Saves.CharacterData characterData) &&
                characterData.WeaponID < Kernel_bin.WeaponsData.Count)
            {
                byte altID = Kernel_bin.WeaponsData[characterData.WeaponID].AltID;
                if (Weapons.TryGetValue(c, out List<byte> weapons) && weapons != null && weapons.Count > altID)
                    weaponId = weapons[altID];
            }

            return weaponId;
        }

        private static void InitBattle()
        {
            if (Stage == null || Stage.Scenario != Memory.Encounters.Scenario || ForceReload)
                using (BinaryReader br = Stage.Open())
                {
                    //Camera and stage are in the same file.
                    Camera = Camera.Read(br);
                    Stage = Stage.Read(Camera.EndOffset, br);
                }
            if (CROSSHAIR == null || ForceReload)
                CROSSHAIR = new IGMDataItem.Icon { Data = Icons.ID.Cross_Hair1 };
            //testQuad = Memory.Icons.Quad(Icons.ID.Cross_Hair1, 2);
            //MakiExtended.Debugger_Spawn();
            //MakiExtended.Debugger_Feed(typeof(Module_battle_debug), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            //InputMouse.Mode = MouseLockMode.Center;
            if (DeadTime == null)
            {
                DeadTime = new DeadTime();
                DeadTime.DoneEvent += DeadTime_DoneEvent;
            }
            DeadTime.Restart();
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.Encounters.ID}\t cEnemies: {Memory.Encounters.EnabledEnemy}\t Enemies: {string.Join(",", Memory.Encounters.BEnemies.Where(x => x != 0x00).Select(x => $"{x}").ToArray())}");
            if (fps_camera == null)
                fps_camera = new FPS_Camera();
            if (RegularPyramid == null)
            {
                RegularPyramid = new Battle.RegularPyramid();
                RegularPyramid.Set(-2.5f, 2f, Color.Yellow);
            }
            RegularPyramid.Hide();
            //init renderer
            if (effect == null)
                effect = new BasicEffect(Memory.graphics.GraphicsDevice);

            camTarget = new Vector3(41.91198f, 33.59995f, 6.372305f);
            camPosition = new Vector3(40.49409f, 39.70397f, -43.321299f);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               Memory.graphics.GraphicsDevice.Viewport.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camPosition, Vector3.
                          Forward, Vector3.Up);
            battleModule++;
            if (ate == null)
                ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
                {
                    Projection = projectionMatrix,
                    View = viewMatrix,
                    World = worldMatrix
                };
            return;
        }

        private static CharacterInstanceInformation ReadCharacter(ref int cid, Characters c)
        {
            CharacterInstanceInformation cii = new CharacterInstanceInformation
            {
                Data = ReadCharacterData((int)c, GetCostume(c), GetWeaponID(c)),
                animationSystem = new AnimationSystem() { AnimationQueue = new ConcurrentQueue<int>() },
                characterId = cid++,
            };
            //cii.animationSystem.animationId = 4;
            Memory.State[c].BattleStart(cii);
            return cii;
        }

        private static CharacterData ReadCharacterData(int characterId, int alternativeCostumeId, int weaponId)
        {
            Debug_battleDat character = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Character, alternativeCostumeId);
            Debug_battleDat weapon;
            if (characterId == 1 || characterId == 9)
                weapon = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Weapon, weaponId, character);
            else if (weaponId != -1) weapon = Debug_battleDat.Load(characterId, Debug_battleDat.EntityType.Weapon, weaponId);
            else weapon = null;
            return new CharacterData()
            {
                character = character,
                weapon = weapon
            };
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
                if (CharacterInstances != null && !ForceReload)
                {
                    var test = CharacterInstances.Select((x, i) => new { cii = x, index = i, id = (Characters)x.Data.character.id, alt = x.Data.character.altid, weapon = x.Data.weapon.altid });
                    //where characters haven't changed
                    foreach (var x in test.Where(x => Memory.State.Party[x.index] == x.id && (GetCostume(x.id) == x.alt) && x.weapon == GetWeaponID(x.id)))
                    {
                        Memory.State[x.id].BattleStart(x.cii);
                        return;
                    }
                    //where character, weapon, or costume has changed
                    foreach (var x in test.Where(x => !(Memory.State.Party[x.index] == x.id && (GetCostume(x.id) == x.alt) && x.weapon == GetWeaponID(x.id))))
                    {
                        int _cid = x.index;
                        CharacterInstances[x.index] = ReadCharacter(ref _cid, x.id);
                        return;
                    }
                }

                CharacterInstances = new List<CharacterInstanceInformation>(3);
                int cid = 0;
                foreach (Characters c in Memory.State.Party)
                {
                    if (c != Characters.Blank)
                    {
                        CharacterInstances.Add(ReadCharacter(ref cid, c));
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

        private static void ReadData()
        {
            ReadCharacters();
            ReadMonster();

            battleModule++;
        }

        /// <summary>
        /// This method is responsible to read/parse the enemy data. It holds the result in
        /// monstersData[] This method was designed to read only one instance of enemy. A list called
        /// EnemyInstance holds data information for each enemy
        /// </summary>
        private static void ReadMonster()
        {
            int r(int i) => 7 - i;
            Encounter encounter = Memory.Encounters.Current;

            if (encounter.EnabledEnemy.Cast<bool>().Any(x => x))
            {
                IEnumerable<byte> monstersList = encounter.BEnemies.Select((x, i) => new { i, x }).Where(x => encounter.EnabledEnemy[r(x.i)])?.Select(x => x.x).Distinct();
                monstersData = monstersList?.Select(x => Debug_battleDat.Load(x, Debug_battleDat.EntityType.Monster)).ToArray();
            }
            else
            {
                monstersData = new Debug_battleDat[0];
                return;
            }

            Enemy.Party = encounter.BEnemies.Select((x, i) => new { i, x }).Where(x => encounter.EnabledEnemy[r(x.i)]).Select(x =>
            {
                Debug_battleDat debug_battleDat = monstersData.FirstOrDefault(mon => mon.GetId == x.x);
                int i = r(x.i);
                return debug_battleDat == default
                    ? null
                    : (Enemy)new EnemyInstanceInformation()
                    {
                        Data = debug_battleDat,
                        bIsHidden = encounter.HiddenEnemies[i],
                        bIsActive = true,
                        partypos = (sbyte)(-1 - x.i),
                        bIsUntargetable = encounter.UntargetableEnemy[i],
                        animationSystem = new AnimationSystem() { AnimationQueue = new ConcurrentQueue<int>() },
                        Location = encounter.enemyCoordinates[i], //each instance needs own location.
                        FixedLevel = encounter.bLevels[i]
                    };
            }).Where(x => x != null).ToList();
        }

        private static void ResetTime() => FrameTime = TimeSpan.FromTicks(FrameTime.Ticks % FPS.Ticks);

        private static void StartAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.animationSystem.StartAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.animationSystem.StartAnimation();
        }

        private static void StopAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.animationSystem.StopAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.animationSystem.StopAnimation();
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

        /// <summary>
        /// if (12 >= [0..255]) Gilgamesh is summoned (5.1 %)
        /// </summary>
        /// <returns>bool</returns>
        private static bool TestGilgamesh() =>

            Memory.State.BattleMISCIndicator.HasFlag(Saves.Data.MiscIndicator.Gilgamesh) && Memory.Random.Next(256) <= 12;

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

                if (CharacterInstances != null)
                    foreach (CharacterInstanceInformation cii in CharacterInstances)
                    {
                        if (!cii.animationSystem.AnimationStopped && (!Memory.State[cii.VisibleCharacter]?.IsPetrify ?? true))
                            cii.animationSystem.NextFrame();
                    }

                ResetTime();
            }
        }

        #endregion Methods

        #region Structs

        /// <summary>
        /// Animation system. Decided to go for struct, so I can attach it to instance and manipulate
        /// easily grouped. It's also open for modifications
        /// </summary>
        public struct AnimationSystem
        {
            #region Fields

            public ConcurrentQueue<int> AnimationQueue;

            private int _animationFrame;

            private int _animationId;

            private int _lastAnimationFrame;

            private int _lastAnimationId;

            private bool bAnimationStopped;

            #endregion Fields

            #region Properties

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

            public int AnimationId
            {
                get => _animationId; set
                {
                    _lastAnimationId = _animationId;
                    _animationId = value;
                    AnimationFrame = 0;
                }
            }

            public bool AnimationStopped => bAnimationStopped;

            public int LastAnimationFrame { get => _lastAnimationFrame; private set => _lastAnimationFrame = value; }

            public int LastAnimationId { get => _lastAnimationId; private set => _lastAnimationId = value; }

            #endregion Properties

            #region Methods

            public int NextFrame() => ++AnimationFrame;

            public bool StartAnimation() => bAnimationStopped = false;

            public bool StopAnimation()
            {
                LastAnimationFrame = AnimationFrame;
                AnimationId = AnimationId;
                return bAnimationStopped = true;
            }

            #endregion Methods
        }

        public struct CharacterData
        {
            #region Fields

            public Debug_battleDat character, weapon;

            public Vector3 Location { get; internal set; }

            #endregion Fields
        };

        #endregion Structs

        #region Classes

        /// <summary>
        /// CharacterInstanceInformation should only be used for battle-exclusive data. Manipulating
        /// HP, GFs, junctions and other character-specific things should happen outside battle,
        /// because such information about characters is shared between almost all modules. This
        /// field contains information about the current status of battle rendering like animation
        /// frames/ rendering flags/ effects attached
        /// </summary>
        public class CharacterInstanceInformation
        {
            #region Fields

            public AnimationSystem animationSystem;
            public bool bIsHidden;
            public int characterId;
            public CharacterData Data;

            #endregion Fields

            #region Properties

            //0 is Whatever guy
            public Characters VisibleCharacter => (Characters)Data.character.GetId;

            #endregion Properties

            #region Methods

            //GF sequences, magic...
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

            #endregion Methods
        }

        public class EnemyInstanceInformation
        {
            #region Fields

            public AnimationSystem animationSystem;
            public bool bIsActive;
            public bool bIsHidden;
            public bool bIsUntargetable;
            public Debug_battleDat Data;

            /// <summary>
            /// bit position of the enemy in encounter data. Use to pair the information with
            /// encounter data
            /// </summary>
            public sbyte partypos;

            public Coordinate Location { get; internal set; }
            public byte FixedLevel { get; internal set; }
            public bool IsFixedLevel => FixedLevel != 0xFF;

            #endregion Fields
        }

        #endregion Classes
    }
}