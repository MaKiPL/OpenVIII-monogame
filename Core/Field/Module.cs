using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    //issues found.
    //558 //color looks off on glow. with purple around it.
    //267 //text showing with white background.
    //132 missing lava.
    //pupu states that there is are 2 widths of the texture for Type1 and Type2
    //we are only using 1 so might be reading the wrong pixels somewhere.
    public static class Module
    {
        #region Fields

        private static Archive Archive;

        #endregion Fields

        #region Enums

        [Flags]
        public enum _Toggles : byte
        {
            DumpingData = 0x1,
            ClassicSpriteBatch = 0x2,
            Quad = 0x4,
            WalkMesh = 0x8,
            Deswizzle = 0x10,
            Perspective = 0x20,
            Menu = 0x40,
        }

        #endregion Enums

        #region Properties

        public static Background Background => Archive?.Background;
        public static Cameras Cameras => Archive?.Cameras;
        private static EventEngine EventEngine => Archive?.EventEngine;
        public static FieldMenu FieldMenu { get; private set; }
        private static INF inf => Archive?.inf;
        public static FF8String AreaName => Archive?.GetAreaNames()?.FirstOrDefault();

        public static ushort GetForcedBattleEncounter
        {
            get
            {
                HashSet<ushort> t = Archive?.GetForcedBattleEncounters();
                if (t == null || t.Count == 0)
                    return ushort.MaxValue;
                return t.First();
            }
        }

        public static Field_modes Mod
        {
            get => Archive.Mod; private set => Archive.Mod = value;
        }

        public static MrtRat MrtRat => Archive.MrtRat;

        private static MSK msk => Archive.msk;

        public static PMP pmp => Archive.pmp;

        private static IServices services => Archive.services;

        private static SFX sfx => Archive.sfx;

        private static TDW tdw => Archive.tdw;

        public static _Toggles Toggles { get; set; } = _Toggles.Quad | _Toggles.Menu;

        public static WalkMesh WalkMesh => Archive.WalkMesh;

        #endregion Properties

        #region Methods

        public static void Draw()
        {
            switch (Mod)
            {
                case Field_modes.INIT:
                    break; //null
                default:
                    Archive.Draw();
                    if (Toggles.HasFlag(_Toggles.Menu))
                        FieldMenu.Draw();
                    break;

                case Field_modes.DISABLED:
                    FieldMenu.Draw();
                    break;
            }
        }

        public static string GetFieldName()
        {
            string fieldname = Memory.FieldHolder.fields[Memory.FieldHolder.FieldID].ToLower();
            if (string.IsNullOrWhiteSpace(fieldname))
                fieldname = $"unk{Memory.FieldHolder.FieldID}";
            return fieldname;
        }

        public static string GetFolder(string fieldname = null, string subfolder = "")
        {
            if (string.IsNullOrWhiteSpace(fieldname))
                fieldname = GetFieldName();
            string folder = Path.Combine(Path.GetTempPath(), "Fields", fieldname.Substring(0, 2), fieldname, subfolder);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static void ResetField()
        {
            Memory.SuppressDraw = true;
            if (Archive != null)
                Mod = Field_modes.INIT;
        }

        public static void Update()
        {
            if (Input2.DelayedButton(Keys.D0))
                Toggles = Toggles.Flip(_Toggles.Menu);
            else
            {
                if (Archive == null)
                    Archive = new Archive();
                switch (Mod)
                {
                    case Field_modes.INIT:
                        Archive.Init();
                        if (FieldMenu == null)
                            FieldMenu = FieldMenu.Create();
                        FieldMenu.Refresh();
                        break;

                    case Field_modes.DEBUGRENDER:
                        Archive.Update();
                        if (Toggles.HasFlag(_Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case Field_modes.NOJSM://no scripts but has background.
                        Archive.Update();
                        if (Toggles.HasFlag(_Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case Field_modes.DISABLED:
                        FieldMenu.Update();
                        break;
                }
            }
        }

        #endregion Methods
    }
}