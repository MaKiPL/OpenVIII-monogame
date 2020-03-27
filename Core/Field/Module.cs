using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
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

        private static Archive _archive;

        #endregion Fields

        #region Properties

        public static FF8String AreaName => _archive?.GetAreaNames()?.FirstOrDefault();
        public static Background Background => _archive?.Background;
        /*
                public static Cameras Cameras => _archive?.Cameras;
        */
        /*
                private static EventEngine EventEngine => _archive?.EventEngine;
        */
        public static FieldMenu FieldMenu { get; set; }
        /*
                private static INF INF => _archive?.INF;
        */
        /*
                public static ushort GetForcedBattleEncounter
                {
                    get
                    {
                        HashSet<ushort> t = _archive?.GetForcedBattleEncounters();
                        if (t == null || t.Count == 0)
                            return ushort.MaxValue;
                        return t.First();
                    }
                }
        */

        public static FieldModes Mod
        {
            get => _archive.Mod; private set => _archive.Mod = value;
        }

        /*
                public static MrtRat MrtRat => _archive.MrtRat;
        */

        /*
                private static MSK MSK => _archive.MSK;
        */

        public static PMP PMP => _archive.PMP;

        /*
                private static IServices Services => _archive.Services;
        */

        /*
                private static SFX SFX => _archive.SFX;
        */

        /*
                private static TDW TDW => _archive.TDW;
        */

        public static Toggles Toggles { get; set; } = Toggles.Quad | Toggles.Menu | Toggles.DumpingData;

        public static WalkMesh WalkMesh => _archive.WalkMesh;

        #endregion Properties

        #region Methods

        public static void Draw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            switch (Mod)
            {
                case FieldModes.Init:
                    break; //null
                case FieldModes.DebugRender:
                case FieldModes.NoJSM:
                    _archive.Draw();
                    if (Toggles.HasFlag(Toggles.Menu))
                        FieldMenu.Draw();
                    break;

                case FieldModes.Disabled:
                    FieldMenu.Draw();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetFieldName()
        {
            var fieldName = Memory.FieldHolder.fields[Memory.FieldHolder.FieldID].ToLower();
            if (string.IsNullOrWhiteSpace(fieldName))
                fieldName = $"unk{Memory.FieldHolder.FieldID}";
            return fieldName;
        }

        public static string GetFolder(string fieldName = null, string subfolder = "")
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                fieldName = GetFieldName();
            var folder = Path.Combine(Path.GetTempPath(), "Fields", fieldName.Substring(0, 2), fieldName, subfolder);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static void ResetField()
        {
            Memory.SuppressDraw = true;
            if (_archive != null)
                Mod = FieldModes.Init;
        }

        public static void Update()
        {
            if (Input2.DelayedButton(Keys.D0))
                Toggles = Toggles.Flip(Toggles.Menu);
            else
            {
                if (_archive == null)
                    _archive = new Archive();
                switch (Mod)
                {
                    case FieldModes.Init:
                        var init = _archive.Init();
                        if (init && Mod == FieldModes.Init)
                            Mod++;
                        if (FieldMenu == null)
                            FieldMenu = FieldMenu.Create();
                        FieldMenu.Refresh();
                        break;

                    case FieldModes.DebugRender:
                        _archive.Update();
                        if (Toggles.HasFlag(Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case FieldModes.NoJSM://no scripts but has background.
                        _archive.Update();
                        if (Toggles.HasFlag(Toggles.Menu))
                            FieldMenu.Update();
                        break; //await events here
                    case FieldModes.Disabled:
                        FieldMenu.Update();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion Methods
    }
}