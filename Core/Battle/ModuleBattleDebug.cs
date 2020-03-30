using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Battle;
using OpenVIII.Battle.Dat;
using OpenVIII.IGMDataItem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII
{
    public static class ModuleBattleDebug
    {
        #region Fields

        public const int YOffset = 0;
        public static AlphaTestEffect Ate;
        public static bool PauseATB = false;

        //parses battle stage and all monsters
        //draw geometry also supports updateCamera
        private static readonly TimeSpan FPS = TimeSpan.FromMilliseconds(1000.0d / 15d);

        private static readonly Vector3 PyramidOffset = new Vector3(0, 3f, 0);
        private static bool _bUseFPSCamera;
        private static Vector3 _camPosition, _camTarget;

        //private static List<EnemyInstanceInformation> EnemyInstances;
        private static List<CharacterInstanceInformation> _characterInstances;

        private static Icon _crossHair;
        private static DeadTime _deadTime;
        private static float _degrees = 90;
        private static bool _forceReload;
        private static FPS_Camera _fpsCamera;
        private static TimeSpan _frameTime = TimeSpan.Zero;
        private static MonsterDatFile[] _monstersData;
        private static sbyte? _partyPos;
        private static RegularPyramid _regularPyramid;
        private static byte _sid;
        private static readonly ConcurrentDictionary<Characters, IReadOnlyList<byte>> SWeapons = new ConcurrentDictionary<Characters, IReadOnlyList<byte>>();

        #endregion Fields

        #region Enums

        /// <summary>
        /// Main Animations IDs
        /// </summary>
        /// <remarks>more beyond this maybe part of attacking and such.</remarks>
        /// <see cref="http://forums.qhimm.com/index.php?topic=19362.msg269777#msg269777"/>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum AnimID
        {
            Idle = 0,
            Critical = 1,
            Dead = 2
            //These seem to be referring to something else.
            //Like he says there's another section with 30 sequences.
            //where it tells how to chain the animations together.

            //Damage_Taken_low_hp,
            //Damage_Taken_normal,
            //Damage_Taken_critical,
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

        public static BattleModule BattleModule { get; set; }
        public static Camera Camera { get; private set; }

        public static ConcurrentDictionary<Characters, IReadOnlyList<byte>> Costumes { get; } = new ConcurrentDictionary<Characters, IReadOnlyList<byte>>();

        public static int DebugFrame { get; private set; }
        public static BasicEffect Effect { get; private set; }

        public static Matrix ProjectionMatrix { get; private set; }

        public static Stage Stage { get; private set; }
        public static Matrix ViewMatrix { get; private set; }

        public static ConcurrentDictionary<Characters, IReadOnlyList<byte>> Weapons
        {
            get
            {
                FillWeapons();
                lock (WeaponLock)
                {
                    return SWeapons;
                }
            }
        }

        public static Matrix WorldMatrix { get; private set; }

        private static Vector3 CenterOfScreen => Memory.Graphics.GraphicsDevice.Viewport.Unproject(
            new Vector3(Memory.Graphics.GraphicsDevice.Viewport.Width / 2f,
                Memory.Graphics.GraphicsDevice.Viewport.Height / 2f, 0f), ProjectionMatrix, ViewMatrix, WorldMatrix);

        #endregion Properties

        #region Methods

        public static void AddAnimationToQueue(EntityType entityType, int nIndex, int newAnimId)
        {
            switch (entityType)
            {
                case EntityType.Monster:
                    if (Enemy.Party[nIndex].EII.Data.Animations.Count > newAnimId)
                        Enemy.Party[nIndex].EII.AnimationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    if (Math.Max(_characterInstances[nIndex].Data.Character.Animations.Count,
                        _characterInstances[nIndex].Data.Weapon.Animations.Count) > newAnimId)
                        _characterInstances[nIndex].AnimationSystem.AnimationQueue.Enqueue(newAnimId);
                    return;

                default:
                    return;
            }
        }

        public static void AddSequenceToQueue(EntityType entityType, int nIndex, AnimationSequence section5)
        {
            foreach (var newAnimId in section5)
            {
                AddAnimationToQueue(entityType, nIndex, newAnimId);
            }
        }

        public static Matrix CreateBillboard(Vector3 pos) => Matrix.CreateBillboard(pos, CenterOfScreen, Vector3.Up, null);

        /*
                public static Matrix CreateConstrainedBillboard(Vector3 pos, Vector3 rotateAxis) => Matrix.CreateConstrainedBillboard(pos, CenterOfScreen, rotateAxis, null, null);
        */

        public static void Draw()
        {
            Memory.SpriteBatch.GraphicsDevice.Clear(Color.Black);
            switch (BattleModule)
            {
                case BattleModule.DrawGeometry:
                    //DrawGeometry();
                    DrawMonsters();
                    DrawCharactersWeapons();
                    _regularPyramid.Draw(WorldMatrix, ViewMatrix, ProjectionMatrix);
                    Stage.Draw();
                    var v = GetIndicatorPoint(-1);
                    v.Y -= 5f;
                    if (!_bUseFPSCamera)
                        Menu.BattleMenus.Draw();
                    break;

                case BattleModule.Active:
                    break;

                case BattleModule.Init:
                    break;

                case BattleModule.ReadData:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            if(Memory.GameTime!=null)
                Memory.ImGui.BeforeLayout(Memory.GameTime);
            ImGui.Begin("BATTLE DEBUG INFO");
            ImGui.Text($"Encounter ready at: {Memory.Encounters.ID} - {Memory.Encounters.Filename}");
            ImGui.Text($"Debug variable: {DebugFrame} ({DebugFrame >> 4},{DebugFrame & 0b1111})\n");
            ImGui.Text($"1000/deltaTime milliseconds: {(Memory.ElapsedGameTime.TotalSeconds > 0 ? 1d / Memory.ElapsedGameTime.TotalSeconds : 0d)}\n");
            ImGui.Text($"Average FrameRate: {FPSCounter.AverageFramesPerSecond}\n");
            ImGui.Text($"camera frame: {Camera.cam.CurrentTime}/{Camera.cam.TotalTime}\n");
            ImGui.Text($"Camera.World.Position: {Extended.RemoveBrackets(_camPosition.ToString())}\n");
            ImGui.Text($"Camera.World.Target: {Extended.RemoveBrackets(_camTarget.ToString())}\n");
            ImGui.Text($"Camera.FOV: {MathHelper.Lerp(Camera.cam.startingFOV, Camera.cam.endingFOV, Camera.cam.CurrentTime.Ticks / (float)Camera.cam.TotalTime.Ticks)}\n");
            ImGui.Text($"Camera.Mode: {Camera.cam.control_word & 1}\n");
            ImGui.Text($"DEBUG: Press 0 to switch between FPSCamera/Camera anim: {_bUseFPSCamera}\n");
            ImGui.Text($"Sequence ID: {_sid}, press F10 to activate sequence, F11 SID--, F12 SID++");
            ImGui.End();
            Memory.ImGui.AfterLayout();
        }

        public static void DrawCrossHair(Enemy enemy)
        {
            var shot = Menu.BattleMenus.GetCurrentBattleMenu()?.Shot;
            if (shot == null || !shot.Enabled) return;
            var targets = shot.Targets;
            if (targets == null) return;
            foreach (var d in targets)
            {
                if (!d.GetEnemy(out var e) || !e.Equals(enemy)) continue;
                var posIn3DSpace = e.EII.Data.IndicatorPoint(e.EII.Location);
                posIn3DSpace.Y -= 1f;
                var screenPos = Memory.Graphics.GraphicsDevice.Viewport.Project(posIn3DSpace, ProjectionMatrix, ViewMatrix, WorldMatrix);
                Memory.SpriteBatchStartAlpha();
                _crossHair.Pos = new Rectangle(new Vector2(screenPos.X, screenPos.Y).ToPoint(), Point.Zero);
                var icons = Memory.Icons[_crossHair.Data];
                var texture = Memory.Icons.GetTexture(Icons.ID.Cross_Hair1);
                var s = texture.ScaleFactor;
                _crossHair.Pos.Offset(-icons.Width * s.X / 2f, -icons.Height * s.Y / 2f);
                _crossHair.Draw();
                Memory.SpriteBatchEnd();
                break;
            }
        }

        public static Vector3 GetIndicatorPoint(int n)
        {
            if (n >= 0)
            {
                if ((_characterInstances == null))
                    return Vector3.Zero;

                return _characterInstances[n].Data.Character.IndicatorPoint(_characterInstances[n].Data.Location) +
                       PyramidOffset;
            }

            if (Enemy.Party == null) return Vector3.Zero;
            var enemyInstanceInformation = Enemy.Party.FirstOrDefault(x => x.EII.PartyPos == n)?.EII;
            if (enemyInstanceInformation == null) return Vector3.Zero;
            return enemyInstanceInformation.Data.IndicatorPoint(enemyInstanceInformation.Location) + PyramidOffset;
        }

        public static void Inputs()
        {
            if (Input2.Button(Keys.D0))
                _bUseFPSCamera = !_bUseFPSCamera;
            else if (Input2.Button(Keys.D1))
            {
                if ((DebugFrame & 0b1111) >= 7)
                {
                    DebugFrame += 0b00010000;
                    DebugFrame -= 7;
                }
                else DebugFrame += 1;
                Camera.ChangeAnimation((byte)DebugFrame);
            }
            else if (Input2.Button(Keys.D2))
            {
                if ((DebugFrame & 0b1111) == 0)
                {
                    DebugFrame -= 0b00010000;
                    DebugFrame += 7;
                }
                else DebugFrame--;
                Camera.ChangeAnimation((byte)DebugFrame);
            }
            else if (Input2.Button(Keys.F5))
            {
                ResetState();
            }
            else if (Input2.Button(Keys.D3))
            {
                BattleModule = BattleModule.Init;
                Memory.Encounters.Previous();
                Memory.SuppressDraw = true;
            }
            else if (Input2.Button(Keys.D4))
            {
                BattleModule = BattleModule.Init;
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
                if (_sid < 255)
                    _sid++;
                else _sid = 0;
            }
            else if (Input2.Button(Keys.F11))
            {
                if (_sid <= 0)
                    _sid = 255;
                else _sid--;
            }
            else if (Input2.Button(Keys.F10))
            {
                AddSequenceToAllQueues(_sid);
            }
            else if (Input2.Button(Keys.F9))
            {
                //AddSequenceToAllQueues(new AnimationSequence
                //{
                //    AnimationQueue = new List<byte> {
                //    //0x2,
                //    //0x5,
                //    //0xf,
                //    //0x10,
                //    //0xb,
                //    //0x3,
                //    //0x6,
                //    0xe,
                //    //0x1,
                //    0xf,
                //    0x0
                //}
                //});
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
        /// Plays requested animation for given entity immediately (without waiting for current
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
                    var mInstanceInformationProvider = Enemy.Party[nIndex].EII;
                    mInstanceInformationProvider.AnimationSystem.AnimationId = newAnimId;
                    Enemy.Party[nIndex].EII = mInstanceInformationProvider;
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    var cInstanceInformationProvider = _characterInstances[nIndex];
                    cInstanceInformationProvider.AnimationSystem.AnimationId = newAnimId;
                    _characterInstances[nIndex] = cInstanceInformationProvider;
                    return;

                default:
                    return;
            }
        }

        public static void ResetState()
        {
            //reload
            BattleModule = BattleModule.Init;
            _forceReload = true;
            Memory.SuppressDraw = true;
        }

        public static void Update()
        {
            if (_characterInstances != null)
                foreach (var cii in _characterInstances)
                {
                    var c = Memory.State?[cii.VisibleCharacter];
                    if (c == null) continue;
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
                foreach (var e in Enemy.Party)
                    e.Update(); //updates ATB for enemy.
            var ret = false;
            switch (BattleModule)
            {
                case BattleModule.Init:
                    Memory.SuppressDraw = true;
                    InitBattle();
                    break;

                case BattleModule.ReadData:
                    Memory.SuppressDraw = true;
                    ReadData();
                    Menu.BattleMenus.Refresh();
                    Menu.FadeIn();
                    _forceReload = false;
                    break;

                case BattleModule.DrawGeometry:
                    Stage?.Update();
                    _deadTime?.Update();
                    Menu.BattleMenus.Update();
                    var partyPos = Menu.BattleMenus.PartyPos;
                    _regularPyramid.Set(GetIndicatorPoint(partyPos ?? 0));
                    if (partyPos != _partyPos)
                    {
                        if (partyPos == null)
                            _regularPyramid.FadeOut();
                        else
                            _regularPyramid.FadeIn();
                        _partyPos = partyPos;
                    }
                    if (_bUseFPSCamera)
                    {
                        ViewMatrix = _fpsCamera.Update(ref _camPosition, ref _camTarget, ref _degrees);
                        ProjectionMatrix = Camera.projectionMatrix;
                    }
                    else
                    {
                        if (Camera != null)
                        {
                            Camera.Update();
                            ViewMatrix = Camera.viewMatrix;
                            ProjectionMatrix = Camera.projectionMatrix;
                        }

                        ret = Menu.BattleMenus.Inputs();
                    }
                    break;

                case BattleModule.Active:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!ret) Inputs();
            _regularPyramid.Update();
            UpdateFrames();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static void AddSequenceToAllQueues(AnimationSequence section5)
        {
            for (var i = 0; i < Enemy.Party.Count; i++)
            {
                AddSequenceToQueue(EntityType.Monster, i, section5);
            }
            for (var i = 0; i < _characterInstances.Count; i++)
            {
                AddSequenceToQueue(EntityType.Character, i, section5);
            }
        }

        private static void AddSequenceToAllQueues(byte sid)
        {
            AnimationSequence section5;
            for (var i = 0; i < Enemy.Party.Count; i++)
            {
                if (Enemy.Party[i].EII.Data.Sequences.Count <= sid) continue;
                section5 = Enemy.Party[i].EII.Data.Sequences.FirstOrDefault(x => x.ID == sid);

                AddSequenceToQueue(EntityType.Monster, i, section5);
                //AddAnimationToQueue(Debug_battleDat.EntityType.Monster, i, 0);
            }
            for (var i = 0; i < _characterInstances.Count; i++)
            {
                var weapon = _characterInstances[i].Data.Weapon;
                var character = _characterInstances[i].Data.Character;
                var sequences =
                    (weapon?.Sequences.Count ?? 0) == 0 ? character.Sequences : weapon.Sequences;
                if (sequences.Count <= sid) continue;
                section5 = sequences.FirstOrDefault(x => x.ID == sid);
                AddSequenceToQueue(EntityType.Character, i, section5);
                //AddAnimationToQueue(Debug_battleDat.EntityType.Character, i, 0);
            }
        }

        private static bool CharacterInstanceAnimationStopped(int n) =>
            _characterInstances[n].AnimationSystem.AnimationStopped ||
            ((Memory.State?[(Characters)_characterInstances[n].CharacterId]?.IsPetrify ?? false) &&
            _characterInstances[n].AnimationSystem.StopAnimation());

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
                    animationSystem = Enemy.Party[n].EII.Data.Animations[Enemy.Party[n].EII.AnimationSystem.AnimationId];
                    if (Enemy.Party[n].EII.AnimationSystem.AnimationFrame < animationSystem.Count) return;
                    var eInstanceInformationProvider = Enemy.Party[n].EII;
                    if (Enemy.Party[n].EII.AnimationSystem.AnimationQueue.TryDequeue(out var animationID) &&
                        animationID < eInstanceInformationProvider.Data.Animations.Count &&
                        animationID >= 0
                    )
                    {
                        eInstanceInformationProvider.AnimationSystem.AnimationId = animationID;
                    }
                    Enemy.Party[n].EII = eInstanceInformationProvider;
                    return;

                case EntityType.Character:
                case EntityType.Weapon:
                    animationSystem = _characterInstances[n].Data.Character.Animations[_characterInstances[n].AnimationSystem.AnimationId];
                    if (_characterInstances[n].AnimationSystem.AnimationFrame < animationSystem.Count) return;
                    var cInstanceInformationProvider = _characterInstances[n];
                    if (_characterInstances[n].AnimationSystem.AnimationQueue.TryDequeue(out animationID) &&
                        (animationID < cInstanceInformationProvider.Data.Character.Animations.Count ||
                         animationID < (cInstanceInformationProvider.Data.Weapon?.Animations.Count ?? 0)) &&
                        animationID >= 0)
                    {
                        cInstanceInformationProvider.AnimationSystem.AnimationId = animationID;
                    }
                    _characterInstances[n] = cInstanceInformationProvider;
                    return;

                default:
                    return;
            }
        }

        private static void DrawBattleDat(DatFile battleDatFile, double step, ref AnimationSystem animationSystem,
            ref Vector3 position, Quaternion? nullRotation = null)
        {
            if (battleDatFile.Geometry == null) return;
            for (var i = 0; /*i<1 &&*/ i < battleDatFile.Geometry.CObjects; i++)
            {
                var rotation = nullRotation ?? Quaternion.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);
                var vertexPositionTexturePointersGRP = battleDatFile.GetVertexPositions(
                    i,
                    ref position,
                    rotation,
                    ref animationSystem,
                    step); //DEBUG
                if (vertexPositionTexturePointersGRP.IsNotSet())
                    return;
                for (var k = 0; k < vertexPositionTexturePointersGRP.VPT.Length / 3; k++)
                {
                    Ate.Texture = (Texture2D)battleDatFile.Textures[vertexPositionTexturePointersGRP.TexturePointers[k]];
                    foreach (var pass in Ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                        vertexPositionTexturePointersGRP.VPT, k * 3, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Method to render characters and weapons for them
        /// </summary>
        private static void DrawCharactersWeapons()
        {
            Memory.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Ate.Projection = ProjectionMatrix; Ate.View = ViewMatrix; Ate.World = WorldMatrix;
            Effect.TextureEnabled = true;
            //CHARACTER
            if (_characterInstances == null)
                return;

            for (var n = 0; n < _characterInstances.Count; n++)
            {
                CheckAnimationFrame(EntityType.Character, n);
                var characterPosition = _characterInstances[n].Data.Location = GetCharPos(n);
                DrawBattleDat(_characterInstances[n].Data.Character, CharacterInstanceGenerateStep(n),
                    ref _characterInstances[n].AnimationSystem, ref characterPosition);
                DrawShadow(characterPosition, Ate, .5f);

                //WEAPON
                if (_characterInstances[n].Data.Weapon == null) continue;
                CheckAnimationFrame(EntityType.Weapon, n);
                DrawBattleDat(_characterInstances[n].Data.Weapon, CharacterInstanceGenerateStep(n),
                    ref _characterInstances[n].AnimationSystem, ref characterPosition);
            }
        }

        private static void DrawMonsters()
        {
            Memory.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Ate.Projection = ProjectionMatrix; Ate.View = ViewMatrix; Ate.World = WorldMatrix;
            Effect.TextureEnabled = true;

            if (Enemy.Party == null)
                return;
            for (var n = 0; n < Enemy.Party.Count; n++)
            {
                if (Enemy.Party[n].EII.Data.GetId == 127)
                {
                    //TODO;
                    continue;
                }

                CheckAnimationFrame(EntityType.Monster, n);
                var enemyPosition = GetEnemyPos(n);
                enemyPosition.Y += YOffset;
                DrawBattleDat(Enemy.Party[n].EII.Data, GenerateStep(EnemyInstanceAnimationStopped(n)), ref Enemy.Party[n].EII.AnimationSystem, ref enemyPosition, Quaternion.Identity);
                DrawShadow(enemyPosition, Ate, Enemy.Party[n].EII.Data.Skeleton.GetScale.X / 5);
                DrawCrossHair(Enemy.Party[n]);
            }
        }

        /// <summary>
        /// [BROKEN] See issue #46
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="alphaTestEffect"></param>
        /// <param name="scale"></param>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void DrawShadow(Vector3 enemyPosition, AlphaTestEffect alphaTestEffect, float scale)
        {
            /*
            VertexPositionTexture[] ptCopy = Memory.shadowGeometry.Clone() as VertexPositionTexture[];
            for (int i = 0; i < ptCopy.Length; i++)
                ptCopy[i].Position = Vector3.Transform(ptCopy[i].Position, Matrix.CreateScale(scale));
            for (int i = 0; i < ptCopy.Length; i++)
            {
                (float x, float y, float z) = enemyPosition;

                ptCopy[i].Position = Vector3.Add(ptCopy[i].Position, new Vector3(x, 0.1f, z));
            }

            alphaTestEffect.Texture = Memory.shadowTexture;
            foreach (EffectPass pass in alphaTestEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ptCopy, 0, 8);
            }
*/
        }

        private static bool EnemyInstanceAnimationStopped(int n) =>
            Enemy.Party[n].EII.AnimationSystem.AnimationStopped ||
            (Enemy.Party[n].IsPetrify &&
            Enemy.Party[n].EII.AnimationSystem.StopAnimation());

        private static readonly object CostumeLock = new object();
        private static void FillCostumes()
        {
                lock (CostumeLock)
                {
                    if (Costumes == null || Costumes.Count != 0) return;
                    for (var i = 0; i <= (int)Characters.Ward_Zabac; i++)
                    {
                        //Debug.Assert(i != 10);
                        var costumes = new SortedSet<byte>();
                        var r = new Regex(@"d(" + i.ToString("X") + @")c(\d+)\.dat", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        var aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);

                        foreach (var s in aw.GetListOfFiles().OrderBy(Path.GetFileName, StringComparer.InvariantCultureIgnoreCase).Where(x => x.IndexOf($"d{i:x}c", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            var match = r.Match(s);
                            if (!byte.TryParse(match.Groups[2].Value, out var a)) continue;
                            costumes.Add(a);
                        }

                        Costumes.TryAdd((Characters)i, costumes.ToList().AsReadOnly());
                    }
                }
            
        }

        private static readonly object WeaponLock = new object();
        private static void FillWeapons()
        {
            lock (WeaponLock)
            {
                if (SWeapons == null ||SWeapons.Count != 0) return;
                for (var i = 0; i <= (int)Characters.Ward_Zabac; i++)
                {
                    //Debug.Assert(i != 10);
                    var weapons = new SortedSet<byte>();
                    var r = new Regex(@"d(" + i.ToString("X") + @")w(\d+)\.dat", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    var aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);

                    foreach (var s in aw.GetListOfFiles().OrderBy(Path.GetFileName, StringComparer.InvariantCultureIgnoreCase).Where(x=>x.IndexOf($"d{i:x}w", StringComparison.OrdinalIgnoreCase) >=0))
                    {
                        var match = r.Match(s);
                        if (!byte.TryParse(match.Groups[2].Value, out var a)) continue;
                        weapons.Add(a);
                    }

                    SWeapons.TryAdd((Characters) i, weapons.ToList().AsReadOnly());
                }
            }
        }

        private static double GenerateStep(bool animationStopped)
        {
            if (animationStopped)
                return 1d;
            return (double)_frameTime.Ticks / FPS.Ticks;
        }

        private static Vector3 GetCharPos(int n) => new Vector3(-10 + n * 10, YOffset, -30);

        private static byte GetCostume(Characters c) =>
            Memory.State[c].AlternativeModel == 0 ? Costumes[c].First() : Costumes[c].Last();

        private static Vector3 GetEnemyPos(int n) => Enemy.Party[n].EII.Location;

        private static byte GetWeaponID(Characters c)
        {
            IReadOnlyList<byte> weapons;
            if (c >= Characters.Laguna_Loire)
            {
                if (Weapons.TryGetValue(c, out weapons) && weapons != null && weapons.Count > 0)
                    return weapons[0];
                return 0;
            }
            var characterData = Memory.State[c];
            if (characterData.WeaponID >= Memory.KernelBin.WeaponsData.Count) return 0;
            var altID = Memory.KernelBin.WeaponsData[characterData.WeaponID].AltID;

            if (!Weapons.TryGetValue(c, out weapons) || weapons == null) return 0;
            if (weapons.Count > altID)
                return weapons[altID];
            return weapons.Count>0 ? weapons[weapons.Count - 1] : (byte) 0;
        }

        private static void InitBattle()
        {
            if (Stage == null || Stage.Scenario != Memory.Encounters.Scenario || _forceReload)
                using (var br = Stage.Open())
                {
                    //Camera and stage are in the same file.
                    Camera = Camera.Read(br);
                    Stage = Stage.Read(Camera.EndOffset, br);
                }
            if (_crossHair == null || _forceReload)
                _crossHair = new Icon { Data = Icons.ID.Cross_Hair1 };
            //testQuad = Memory.Icons.Quad(Icons.ID.Cross_Hair1, 2);
            //MakiExtended.Debugger_Spawn();
            //MakiExtended.Debugger_Feed(typeof(Module_battle_debug), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            //InputMouse.Mode = MouseLockMode.Center;
            if (_deadTime == null)
            {
                _deadTime = new DeadTime();
            }
            _deadTime.Restart();
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.Encounters.ID}\t cEnemies: {Memory.Encounters.EnabledEnemy}\t Enemies: {string.Join(",", Memory.Encounters.BEnemies.Where(x => x != 0x00).Select(x => $"{x}").ToArray())}");
            if (_fpsCamera == null)
                _fpsCamera = new FPS_Camera();
            if (_regularPyramid == null)
            {
                _regularPyramid = new RegularPyramid();
                _regularPyramid.Set(-2.5f, 2f, Color.Yellow);
            }
            _regularPyramid.Hide();
            //init renderer
            if (Effect == null)
                Effect = new BasicEffect(Memory.Graphics.GraphicsDevice);

            _camTarget = new Vector3(41.91198f, 33.59995f, 6.372305f);
            _camPosition = new Vector3(40.49409f, 39.70397f, -43.321299f);

            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               Memory.Graphics.GraphicsDevice.Viewport.AspectRatio,
                1f, 1000f);
            ViewMatrix = Matrix.CreateLookAt(_camPosition, _camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            WorldMatrix = Matrix.CreateWorld(_camPosition, Vector3.
                          Forward, Vector3.Up);
            BattleModule++;
            if (Ate == null)
                Ate = new AlphaTestEffect(Memory.Graphics.GraphicsDevice)
                {
                    Projection = ProjectionMatrix,
                    View = ViewMatrix,
                    World = WorldMatrix
                };
        }

        private static CharacterInstanceInformation ReadCharacter(ref int cid, Characters c)
        {
            var cii = new CharacterInstanceInformation
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
            DatFile character = CharacterDatFile.CreateInstance(characterId, alternativeCostumeId);
            DatFile weapon;
            if (characterId == 1 || characterId == 9)
                weapon = WeaponDatFile.CreateInstance(characterId, weaponId, character);
            else if (weaponId != -1) weapon = WeaponDatFile.CreateInstance(characterId, weaponId);
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
                int cid;
                if (_characterInstances != null && !_forceReload)
                {
                    var test = _characterInstances.Select((x, i) => new
                    {
                        cii = x,
                        index = i,
                        id = (Characters)x.Data.Character.ID,
                        alt = x.Data.Character.AltID,
                        weapon = x.Data.Weapon.AltID
                    }).ToArray();
                    //where characters haven't changed
                    foreach (var x in test.Where(x => Memory.State.Party[x.index] == x.id && (GetCostume(x.id) == x.alt) && x.weapon == GetWeaponID(x.id)))
                    {
                        Memory.State[x.id].BattleStart(x.cii);
                        return;
                    }
                    //where character, weapon, or costume has changed
                    foreach (var x in test.Where(x => !(Memory.State.Party[x.index] == x.id && (GetCostume(x.id) == x.alt) && x.weapon == GetWeaponID(x.id))))
                    {
                        cid = x.index;
                        _characterInstances[x.index] = ReadCharacter(ref cid, x.id);
                        return;
                    }
                }

                _characterInstances = new List<CharacterInstanceInformation>(3);
                cid = 0;
                foreach (var c in Memory.State.Party)
                {
                    if (c != Characters.Blank)
                    {
                        _characterInstances.Add(ReadCharacter(ref cid, c));
                    }
                }
            }
            else
                _characterInstances = new List<CharacterInstanceInformation>
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

            BattleModule++;
        }

        /// <summary>
        /// This method is responsible to read/parse the enemy data. It holds the result in
        /// monstersData[] This method was designed to read only one instance of enemy. A list called
        /// EnemyInstance holds data information for each enemy
        /// </summary>
        private static void ReadMonster()
        {
            int r(int i) => 7 - i;
            var encounter = Memory.Encounters.Current;

            if (encounter.EnabledEnemy.Cast<bool>().Any(x => x))
            {
                _monstersData = encounter.UniqueMonstersList?.Select(x => MonsterDatFile.CreateInstance(x)).ToArray();
            }
            else
            {
                _monstersData = null;
                return;
            }

            Enemy.Party = encounter.BEnemies.Select((x, i) => new { i, x }).Where(x => encounter.EnabledEnemy[r(x.i)]).Select(x =>
            {
                DatFile datFile = _monstersData.FirstOrDefault(mon => mon.GetId == x.x);
                var i = r(x.i);
                return datFile == default
                    ? null
                    : (Enemy)new EnemyInstanceInformation
                    {
                        Data = datFile,
                        BIsHidden = encounter.HiddenEnemies[i],
                        BIsActive = true,
                        PartyPos = (sbyte)(-1 - x.i),
                        BIsUntargetable = encounter.UntargetableEnemy[i],
                        AnimationSystem = new AnimationSystem { AnimationQueue = new ConcurrentQueue<int>() },
                        Location = encounter.EnemyCoordinates[i], //each instance needs own location.
                        FixedLevel = encounter.BLevels[i]
                    };
            }).Where(x => x != null).ToList();
        }

        private static void ResetTime() => _frameTime = TimeSpan.FromTicks(_frameTime.Ticks % FPS.Ticks);

        private static void StartAnimations()
        {
            foreach (var c in _characterInstances)
                c.AnimationSystem.StartAnimation();
            foreach (var e in Enemy.Party)
                e.EII.AnimationSystem.StartAnimation();
        }

        private static void StopAnimations()
        {
            foreach (var c in _characterInstances)
                c.AnimationSystem.StopAnimation();
            foreach (var e in Enemy.Party)
                e.EII.AnimationSystem.StopAnimation();
        }

        /// <summary>
        /// Increments animation frames by N, where N is equal to int(deltaTime/FPS). 15FPS is one
        /// frame per ~66 milliseconds. Therefore if deltaTime hits at least: below 33, then frame
        /// gets interpolated above 122, then frame gets skipped (by x/66)
        /// </summary>
        private static void UpdateFrames()
        {
            _frameTime += Memory.ElapsedGameTime;
            if (_frameTime <= FPS) return;
            if (Enemy.Party != null)
                foreach (var e in Enemy.Party.Where(e => !e.EII.AnimationSystem.AnimationStopped && !e.IsPetrify))
                {
                    e.EII.AnimationSystem.NextFrame();
                }

            if (_characterInstances != null)
                foreach (var cii in _characterInstances.Where(cii =>
                    !cii.AnimationSystem.AnimationStopped && (!Memory.State[cii.VisibleCharacter]?.IsPetrify ?? true)))
                {
                    cii.AnimationSystem.NextFrame();
                }

            ResetTime();
        }

        #endregion Methods
    }
}