using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Battle;
using OpenVIII.Battle.Dat;
using OpenVIII.IGMData.Limit;
using OpenVIII.IGMDataItem;
using OpenVIII.Kernel;

namespace OpenVIII
{
    public static class ModuleBattleDebug
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
        private static bool bUseFPSCamera;
        private static Vector3 camPosition, camTarget;

        //private static List<EnemyInstanceInformation> EnemyInstances;
        private static List<CharacterInstanceInformation> CharacterInstances;

        private static DeadTime DeadTime;
        private static float degrees = 90;
        private static BasicEffect effect;
        private static bool ForceReload;
        private static FPS_Camera fps_camera;
        private static TimeSpan FrameTime = TimeSpan.Zero;
        private static DebugBattleDat[] monstersData;
        private static sbyte? partypos;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static Vector3 PyramidOffset = new Vector3(0, 3f, 0);
        private static RegularPyramid RegularPyramid;
        private static ConcurrentDictionary<Characters, List<byte>> s_weapons;
        private static byte SID;

        #endregion Fields

        #region Enums

        /// <summary>
        /// Main Animations IDs
        /// </summary>
        /// <remarks>more beyond this maybe part of attacking and such.</remarks>
        /// <see cref="http://forums.qhimm.com/index.php?topic=19362.msg269777#msg269777"/>
        private enum AnimID
        {
            Idle = 0,
            Critical = 1,
            Dead = 2
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

        public static int battleModule { get; set; }
        public static Camera Camera { get; private set; }
        public static Vector3 CamPosition { get => camPosition; private set => camPosition = value; }

        //Natively the game we are rewriting works in 15 FPS per second
        //-10;
        public static Vector3 CamTarget { get => camTarget; private set => camTarget = value; }

        //basic init stuff; renderer; core
        public static ConcurrentDictionary<Characters, SortedSet<byte>> Costumes { get; private set; }

        public static int DEBUGframe { get; private set; }
        public static BasicEffect Effect { get => effect; private set => effect = value; }
        public static Matrix ProjectionMatrix { get => projectionMatrix; private set => projectionMatrix = value; }
        public static Stage Stage { get; set; }
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

        private static Icon CROSSHAIR { get; set; }


        #endregion Properties

        #region Methods

        public static void AddAnimationToQueue(EntityType entityType, int nIndex, int newAnimId)
        {
            switch (entityType)
            {
                case EntityType.Monster:
                    Enemy.Party[nIndex].EII.AnimationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    CharacterInstances[nIndex].AnimationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                default:
                    return;
            }
        }

        public static void AddSequenceToQueue(EntityType entityType, int nIndex, AnimationSequence section5)
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

            Memory.imgui.BeforeLayout(Memory.GameTime);
            ImGui.Begin("BATTLE DEBUG INFO");
            ImGui.Text($"Encounter ready at: {Memory.Encounters.ID} - {Memory.Encounters.Filename}");
            ImGui.Text($"Debug variable: {DEBUGframe} ({DEBUGframe >> 4},{DEBUGframe & 0b1111})\n");
            ImGui.Text($"1000/deltaTime milliseconds: {(Memory.ElapsedGameTime.TotalSeconds > 0 ? 1d / Memory.ElapsedGameTime.TotalSeconds : 0d)}\n");
            ImGui.Text($"Average FrameRate: {FPSCounter.AverageFramesPerSecond}\n");
            ImGui.Text($"camera frame: {Camera.cam.CurrentTime}/{Camera.cam.TotalTime}\n");
            ImGui.Text($"Camera.World.Position: {Extended.RemoveBrackets(camPosition.ToString())}\n");
            ImGui.Text($"Camera.World.Target: {Extended.RemoveBrackets(camTarget.ToString())}\n");
            ImGui.Text($"Camera.FOV: {MathHelper.Lerp(Camera.cam.startingFOV, Camera.cam.endingFOV, Camera.cam.CurrentTime.Ticks / (float)Camera.cam.TotalTime.Ticks)}\n");
            ImGui.Text($"Camera.Mode: {Camera.cam.control_word & 1}\n");
            ImGui.Text($"DEBUG: Press 0 to switch between FPSCamera/Camera anim: {bUseFPSCamera}\n");
            ImGui.Text($"Sequence BattleID: {SID}, press F10 to activate sequence, F11 SID--, F12 SID++");
            ImGui.End();
            Memory.imgui.AfterLayout();
        }

        public static void DrawCrosshair(Enemy enemy)
        {
            Shot shot = Menu.BattleMenus.GetCurrentBattleMenu()?.Shot;
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
            if (n >= 0)
            {
                if ((CharacterInstances == null))
                    return Vector3.Zero;

                return CharacterInstances[n].Data.Character.IndicatorPoint(CharacterInstances[n].Data.Location) +
                       PyramidOffset;
            }

            if (Enemy.Party == null) return Vector3.Zero;
                EnemyInstanceInformation enemyInstanceInformation = Enemy.Party.FirstOrDefault(x => x.EII.PartyPos == n)?.EII;
                if (enemyInstanceInformation == null) return Vector3.Zero;
                return enemyInstanceInformation.Data.IndicatorPoint(enemyInstanceInformation.Location) + PyramidOffset;
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
                AddAnimationToQueue(EntityType.Monster, 0, 3);
                AddAnimationToQueue(EntityType.Monster, 0, 0);
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
                AddSequenceToAllQueues(new AnimationSequence
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void PlayAnimationImmediately(EntityType entityType, int nIndex, int newAnimId)
        {
            switch (entityType)
            {
                case EntityType.Monster:
                    EnemyInstanceInformation mInstanceInformationProvider = Enemy.Party[nIndex].EII;
                    mInstanceInformationProvider.AnimationSystem.AnimationId = newAnimId;
                    Enemy.Party[nIndex].EII = mInstanceInformationProvider;
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    CharacterInstanceInformation cInstanceInformationProvider = CharacterInstances[nIndex];
                    cInstanceInformationProvider.AnimationSystem.AnimationId = newAnimId;
                    CharacterInstances[nIndex] = cInstanceInformationProvider;
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
                    if (cii.AnimationSystem.AnimationId < 0 ||
                        cii.AnimationSystem.AnimationId > 2) continue;
                    // this would probably interfere with other animations. I am hoping the limits above will keep it good.
                    if (c.IsDead)
                        cii.SetAnimationID((int)AnimID.Dead);
                    else if (c.IsCritical)
                        cii.SetAnimationID((int)AnimID.Critical);
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
                    sbyte? partyPos = Menu.BattleMenus.PartyPos;
                    RegularPyramid.Set(GetIndicatorPoint(partyPos ?? 0));
                    if (partyPos != partypos)
                    {
                        if (partyPos == null)
                            RegularPyramid.FadeOut();
                        else
                            RegularPyramid.FadeIn();
                        partypos = partyPos;
                    }
                    if (bUseFPSCamera)
                    {
                        viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                        projectionMatrix = Camera.projectionMatrix;
                    }
                    else
                    {
                        if (Camera != null)
                        {
                            Camera.Update();
                            viewMatrix = Camera.viewMatrix;
                            projectionMatrix = Camera.projectionMatrix;
                        }

                        ret = Menu.BattleMenus.Inputs();
                    }
                    break;
            }
            if (!ret) Inputs();
            RegularPyramid.Update();
            UpdateFrames();
        }

        private static void AddSequenceToAllQueues(AnimationSequence section5)
        {
            for (int i = 0; i < Enemy.Party.Count; i++)
            {
                AddSequenceToQueue(EntityType.Monster, i, section5);
            }
            for (int i = 0; i < CharacterInstances.Count; i++)
            {
                AddSequenceToQueue(EntityType.Character, i, section5);
            }
        }

        private static void AddSequenceToAllQueues(byte sid)
        {
            AnimationSequence section5;
            for (int i = 0; i < Enemy.Party.Count; i++)
            {
                if (Enemy.Party[i].EII.Data.Sequences.Count > sid)
                {
                    section5 = Enemy.Party[i].EII.Data.Sequences.FirstOrDefault(x => x.ID == sid);
                    if (section5.AnimationQueue != null)
                        AddSequenceToQueue(EntityType.Monster, i, section5);
                    //AddAnimationToQueue(Debug_battleDat.EntityType.Monster, i, 0);
                }
            }
            for (int i = 0; i < CharacterInstances.Count; i++)
            {
                DebugBattleDat weapon = CharacterInstances[i].Data.Weapon;
                DebugBattleDat character = CharacterInstances[i].Data.Character;
                List<AnimationSequence> sequences;
                if ((weapon?.Sequences.Count ?? 0) == 0)
                {
                    sequences = character.Sequences;
                }
                else sequences = weapon.Sequences;
                if (sequences.Count > sid)
                {
                    section5 = sequences.FirstOrDefault(x => x.ID == sid);
                    if (section5.AnimationQueue != null)
                        AddSequenceToQueue(EntityType.Character, i, section5);
                    //AddAnimationToQueue(Debug_battleDat.EntityType.Character, i, 0);
                }
            }
        }

        private static bool CharacterInstanceAnimationStopped(int n) =>
            CharacterInstances[n].AnimationSystem.AnimationStopped ||
            ((Memory.State?[(Characters)CharacterInstances[n].CharacterId]?.IsPetrify ?? false) &&
            CharacterInstances[n].AnimationSystem.StopAnimation());

        private static double CharacterInstanceGenerateStep(int n) => GenerateStep(CharacterInstanceAnimationStopped(n));

        /// <summary>
        /// This function is responsible for deleting the queue of animation if passed correctly
        /// </summary>
        /// <param name="type"></param>
        /// <param name="n"></param>
        private static void CheckAnimationFrame(EntityType type, int n)
        {
            Animation animationSystem;
            switch (type)
            {
                case EntityType.Monster:
                    animationSystem = Enemy.Party[n].EII.Data.animHeader.animations[Enemy.Party[n].EII.AnimationSystem.AnimationId];
                    if (Enemy.Party[n].EII.AnimationSystem.AnimationFrame < animationSystem.CFrames) return;
                    EnemyInstanceInformation eInstanceInformationProvider = Enemy.Party[n].EII;
                    if (Enemy.Party[n].EII.AnimationSystem.AnimationQueue.TryDequeue(out int animationID) &&
                        animationID < eInstanceInformationProvider.Data.animHeader.animations.Length &&
                        animationID >= 0
                    )
                    {
                        eInstanceInformationProvider.AnimationSystem.AnimationId = animationID;
                    }
                    Enemy.Party[n].EII = eInstanceInformationProvider;
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    animationSystem = CharacterInstances[n].Data.Character.animHeader.animations[CharacterInstances[n].AnimationSystem.AnimationId];
                    if (CharacterInstances[n].AnimationSystem.AnimationFrame < animationSystem.CFrames) return;
                    CharacterInstanceInformation cInstanceInformationProvider = CharacterInstances[n];
                    if (CharacterInstances[n].AnimationSystem.AnimationQueue.TryDequeue(out animationID) &&
                        (animationID < cInstanceInformationProvider.Data.Character.animHeader.animations.Length ||
                         animationID < (cInstanceInformationProvider.Data.Weapon?.animHeader.animations.Length ?? 0)) &&
                        animationID >= 0)
                    {
                        cInstanceInformationProvider.AnimationSystem.AnimationId = animationID;
                    }
                    CharacterInstances[n] = cInstanceInformationProvider;
                    return;

                default:
                    return;
            }
        }

        

        private static void DrawBattleDat(DebugBattleDat battleDat, double step, ref AnimationSystem animationSystem, ref Vector3 position, Quaternion? _rotation = null)
        {
            for (int i = 0; /*i<1 &&*/ i < battleDat.Geometry.CObjects; i++)
            {
                Quaternion rotation = _rotation ?? Quaternion.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);
                VertexPositionTexturePointersGRP vptpg = battleDat.GetVertexPositions(
                    i,
                    ref position,
                    rotation,
                    ref animationSystem,
                    step); //DEBUG
                if (vptpg.IsNotSet())
                    return;
                for (int k = 0; k < vptpg.VPT.Length / 3; k++)
                {
                    ate.Texture = (Texture2D)battleDat.textures.textures[vptpg.TexturePointers[k]];
                    foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                        vertexData: vptpg.VPT, vertexOffset: k * 3, primitiveCount: 1);
                    }
                }
            }
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
                CheckAnimationFrame(EntityType.Character, n);
                Vector3 charaPosition = CharacterInstances[n].Data.Location = GetCharPos(n);
                DrawBattleDat(CharacterInstances[n].Data.Character, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].AnimationSystem, ref charaPosition);
                DrawShadow(charaPosition, ate, .5f);

                //WEAPON
                if (CharacterInstances[n].Data.Weapon != null)
                {
                    CheckAnimationFrame(EntityType.Weapon, n);
                    DrawBattleDat(CharacterInstances[n].Data.Weapon, CharacterInstanceGenerateStep(n), ref CharacterInstances[n].AnimationSystem, ref charaPosition);
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

                CheckAnimationFrame(EntityType.Monster, n);
                Vector3 enemyPosition = GetEnemyPos(n);
                enemyPosition.Y += Yoffset;
                DrawBattleDat(Enemy.Party[n].EII.Data, GenerateStep(EnemyInstanceAnimationStopped(n)), ref Enemy.Party[n].EII.AnimationSystem, ref enemyPosition, Quaternion.Identity);
                DrawShadow(enemyPosition, ate, Enemy.Party[n].EII.Data.Skeleton.GetScale.X / 5);
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
            Enemy.Party[n].EII.AnimationSystem.AnimationStopped ||
            (Enemy.Party[n].IsPetrify &&
            Enemy.Party[n].EII.AnimationSystem.StopAnimation());

        private static void FillCostumes()
        {
            if (Costumes == null)
            {
                Costumes = new ConcurrentDictionary<Characters, SortedSet<byte>>();
                Regex r = new Regex(@"d([\da-fA-F]+)c(\d+)\.dat", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);
                foreach (string s in aw.GetListOfFiles())
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
                    ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);

                    foreach (string s in aw.GetListOfFiles().OrderBy(q => Path.GetFileName(q), StringComparer.InvariantCultureIgnoreCase))
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

        private static byte GetCostume(Characters c) => Memory.State[c].AlternativeModel != 0 ? Costumes[c].First() : Costumes[c].Last();

        private static Vector3 GetEnemyPos(int n) =>
            //Memory.Encounters.enemyCoordinates[Enemy.Party[n].EII.index];
            //Vector3 a = Memory.Encounters.enemyCoordinates.AverageVector;
            //Vector3 m = Memory.Encounters.enemyCoordinates.MidVector;

            //v.x -= (short)m.X;

            //v.z -= (short)a.Z;
            Enemy.Party[n].EII.Location;

        private static byte GetWeaponID(Characters c)
        {
            byte weaponId = 0;
            if (Memory.State.Characters.TryGetValue(c, out Saves.CharacterData characterData) &&
                characterData.WeaponID < Memory.Kernel_Bin.WeaponsData.Count)
            {
                byte altID = Memory.Kernel_Bin.WeaponsData[characterData.WeaponID].AltID;
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
                CROSSHAIR = new Icon { Data = Icons.ID.Cross_Hair1 };
            //testQuad = Memory.Icons.Quad(Icons.BattleID.Cross_Hair1, 2);
            //MakiExtended.Debugger_Spawn();
            //MakiExtended.Debugger_Feed(typeof(Module_battle_debug), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            //InputMouse.Mode = MouseLockMode.Center;
            if (DeadTime == null)
            {
                DeadTime = new DeadTime();
            }
            DeadTime.Restart();
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.Encounters.ID}\t cEnemies: {Memory.Encounters.EnabledEnemy}\t Enemies: {string.Join(",", Memory.Encounters.BEnemies.Where(x => x != 0x00).Select(x => $"{x}").ToArray())}");
            if (fps_camera == null)
                fps_camera = new FPS_Camera();
            if (RegularPyramid == null)
            {
                RegularPyramid = new RegularPyramid();
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
        }

        private static CharacterInstanceInformation ReadCharacter(ref int cid, Characters c)
        {
            CharacterInstanceInformation cii = new CharacterInstanceInformation
            {
                Data = ReadCharacterData((int)c, GetCostume(c), GetWeaponID(c)),
                AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>() },
                CharacterId = cid++
            };
            //cii.animationSystem.animationId = 4;
            Memory.State[c].BattleStart(cii);
            return cii;
        }

        private static CharacterData ReadCharacterData(int characterId, int alternativeCostumeId, int weaponId)
        {
            DebugBattleDat character = DebugBattleDat.Load(characterId, EntityType.Character, alternativeCostumeId);
            DebugBattleDat weapon;
            if (characterId == 1 || characterId == 9)
                weapon = DebugBattleDat.Load(characterId, EntityType.Weapon, weaponId, character);
            else if (weaponId != -1) weapon = DebugBattleDat.Load(characterId, EntityType.Weapon, weaponId);
            else weapon = null;
            return new CharacterData
            {
                Character = character,
                Weapon = weapon
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
                    var test = CharacterInstances.Select((x, i) => new { cii = x, index = i, id = (Characters)x.Data.Character.ID, alt = x.Data.Character.AltID, weapon = x.Data.Weapon.AltID }).ToArray();
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
                new CharacterInstanceInformation
                {
                    Data = ReadCharacterData(0,0,0),
                    AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>()},
                    CharacterId = 0
                },
                new CharacterInstanceInformation
                {
                    Data = ReadCharacterData(1,3,8),
                    AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>()},
                    CharacterId = 1
                },
                new CharacterInstanceInformation
                {
                    Data = ReadCharacterData(2,6,13),
                    AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>()},
                    CharacterId = 2
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
                monstersData = encounter.UniqueMonstersList?.Select(x => DebugBattleDat.Load(x, EntityType.Monster)).ToArray();
            }
            else
            {
                monstersData = new DebugBattleDat[0];
                return;
            }

            Enemy.Party = encounter.BEnemies.Select((x, i) => new { i, x }).Where(x => encounter.EnabledEnemy[r(x.i)]).Select(x =>
            {
                DebugBattleDat debug_battleDat = monstersData.FirstOrDefault(mon => mon.GetId == x.x);
                int i = r(x.i);
                return debug_battleDat == default
                    ? null
                    : (Enemy)new EnemyInstanceInformation
                    {
                        Data = debug_battleDat,
                        BIsHidden = encounter.HiddenEnemies[i],
                        BIsActive = true,
                        PartyPos = (sbyte)(-1 - x.i),
                        BIsUntargetable = encounter.UntargetableEnemy[i],
                        AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>() },
                        Location = encounter.enemyCoordinates[i], //each instance needs own location.
                        FixedLevel = encounter.bLevels[i]
                    };
            }).Where(x => x != null).ToList();
        }

        private static void ResetTime() => FrameTime = TimeSpan.FromTicks(FrameTime.Ticks % FPS.Ticks);

        private static void StartAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.AnimationSystem.StartAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.AnimationSystem.StartAnimation();
        }

        private static void StopAnimations()
        {
            foreach (CharacterInstanceInformation c in CharacterInstances)
                c.AnimationSystem.StopAnimation();
            foreach (Enemy e in Enemy.Party)
                e.EII.AnimationSystem.StopAnimation();
        }
        


        /// <summary>
        /// Increments animation frames by N, where N is equal to int(deltaTime/FPS). 15FPS is one
        /// frame per ~66 miliseconds. Therefore if deltaTime hits at least: below 33, then frame
        /// gets interpolated above 122, then frame gets skipped (by x/66)
        /// </summary>
        private static void UpdateFrames()
        {
            FrameTime += Memory.ElapsedGameTime;
            if (FrameTime > FPS)
            {
                if (Enemy.Party != null)
                    foreach (Enemy e in Enemy.Party)
                    {
                        if (!e.EII.AnimationSystem.AnimationStopped && !e.IsPetrify)
                            e.EII.AnimationSystem.NextFrame();
                    }

                if (CharacterInstances != null)
                    foreach (CharacterInstanceInformation cii in CharacterInstances)
                    {
                        if (!cii.AnimationSystem.AnimationStopped && (!Memory.State[cii.VisibleCharacter]?.IsPetrify ?? true))
                            cii.AnimationSystem.NextFrame();
                    }

                ResetTime();
            }
        }

        #endregion Methods
    }
}