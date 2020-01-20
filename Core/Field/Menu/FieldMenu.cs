using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Currently a menu for field screen to enable or disable elements or test scripts.
    /// </summary>
    public class FieldMenu : Menu
    {
        #region Enums

        private enum Mode
        {
            On
        }

        #endregion Enums

        #region Methods

        public static FieldMenu Create() => Create<FieldMenu>();

        public override bool Inputs()
        {
            bool r = false;
            switch ((Mode)GetMode())
            {
                case Mode.On:
                    r = Data[GetMode()].Inputs() || r;
                    break;
            }
            r = base.Inputs() || r;

            return r;
        }

        protected override void Init()
        {
            //Size = new Vector2(960f, 720f);
            Size = new Vector2(1280f, 720f);
            base.Init();
            Data[Mode.On] = IGMData.FieldDebugControls.Create(new Rectangle(0, 0, 480, 360));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.On);
        }

        #endregion Methods
    }
}

namespace OpenVIII.Fields.IGMData
{
    public class FieldDebugControls : OpenVIII.IGMData.Base
    {
        #region Fields

        private const int totalrows = 8;
        private bool skiprefresh;

        #endregion Fields

        #region Properties

        public IGMDataItem.Text ClassicSpriteBatchMode { get => (IGMDataItem.Text)ITEM[4, 0]; protected set => ITEM[4, 0] = value; }
        public IGMDataItem.Text FieldName { get => (IGMDataItem.Text)ITEM[0, 0]; protected set => ITEM[0, 0] = value; }
        public IGMDataItem.Text FourceDump { get => (IGMDataItem.Text)ITEM[5, 0]; protected set => ITEM[5, 0] = value; }
        public IGMDataItem.Text PerspectiveQuadMode { get => (IGMDataItem.Text)ITEM[3, 0]; protected set => ITEM[3, 0] = value; }
        public IGMDataItem.Text QuadBG { get => (IGMDataItem.Text)ITEM[2, 0]; protected set => ITEM[2, 0] = value; }
        public IGMDataItem.Text WalkMesh { get => (IGMDataItem.Text)ITEM[1, 0]; protected set => ITEM[1, 0] = value; }
        public IGMDataItem.Text Deswizzle { get => (IGMDataItem.Text)ITEM[6, 0]; protected set => ITEM[6, 0] = value; }
        public IGMDataItem.Text Reswizzle { get => (IGMDataItem.Text)ITEM[7, 0]; protected set => ITEM[7, 0] = value; }
        public IGMDataItem.Text MouseLocationIn3D { get => (IGMDataItem.Text)ITEM[Count - 1, 0]; protected set => ITEM[Count - 1, 0] = value; }

        #endregion Properties

        #region Methods

        public static FieldDebugControls Create(Rectangle pos) => Create<FieldDebugControls>(totalrows + 1, 1, new IGMDataItem.Box { Pos = pos }, 1, totalrows);

        public override bool Inputs()
        {
            Memory.IsMouseVisible = true;
            if (Input2.DelayedButton(MouseButtons.MiddleButton))
            {
                Debug.WriteLine($"=== Tiles Under MouseLocation: {Module.Background.MouseLocation} ===");
                foreach (Background.Tile tile in Module.Background.TilesUnderMouse())
                {
                    Debug.WriteLine(tile);
                }
                return true;
            }
            return base.Inputs();
        }

        public override void Inputs_Left()
        {
            if (CURSOR_SELECT == 0)
            {
                if (Memory.FieldHolder.FieldID > 0)
                    Memory.FieldHolder.FieldID--;
                else
                    Memory.FieldHolder.FieldID = checked((ushort)(Memory.FieldHolder.fields.Length - 1));
                Module.ResetField();
            }
            else skipsnd = true;
            base.Inputs_Left();
        }

        public override bool Inputs_OKAY()
        {
            int i = 0;
            if (CURSOR_SELECT == i++)
            {
                Module.Background.Dispose();//force all textures to reload.
                Module.ResetField();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Module._Toggles.WalkMesh);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Module._Toggles.Quad);
                if (Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch))
                    Module.Toggles = Module.Toggles.Flip(Module._Toggles.ClassicSpriteBatch);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                if (Module.Toggles.HasFlag(Module._Toggles.Quad))
                {
                    Module.Toggles = Module.Toggles.Flip(Module._Toggles.Perspective);
                    Refresh();
                }
                else skipsnd = true;
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Module._Toggles.ClassicSpriteBatch);
                if (Module.Toggles.HasFlag(Module._Toggles.Quad))
                    Module.Toggles = Module.Toggles.Flip(Module._Toggles.Quad);
                if (Module.Background.HasSpriteBatchTexturesLoaded)
                    Refresh();
                else
                    Module.ResetField();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Module._Toggles.DumpingData);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Background.Deswizzle();
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Background.Reswizzle();
                Refresh();
            }
            else skipsnd = true;
            return base.Inputs_OKAY() || true;
        }

        public override void Inputs_Right()
        {
            if (CURSOR_SELECT == 0)
            {
                if (Memory.FieldHolder.FieldID < checked((ushort)(Memory.FieldHolder.fields.Length - 1)))
                    Memory.FieldHolder.FieldID++;
                else
                    Memory.FieldHolder.FieldID = 0;
                Module.ResetField();
            }
            else skipsnd = true;
            base.Inputs_Right();
        }

        public override void Refresh()
        {
            if (skiprefresh)
            {
                skiprefresh = false;
                return;
            }
                FieldName.Data = $"Field: { Memory.FieldHolder.FieldID} - { Memory.FieldHolder.GetString()?.ToUpper()}";

            BLANKS[0] = false;
            if (Module.Mod != Module.Field_mods.DISABLED)
            {
                WalkMesh.Data = $"Draw WalkMesh: {Module.Toggles.HasFlag(Module._Toggles.WalkMesh)}";
                BLANKS[1] = false;
                QuadBG.Data = $"Draw Quad BG: {Module.Toggles.HasFlag(Module._Toggles.Quad)}";
                BLANKS[2] = false;

                PerspectiveQuadMode.Data = $"Perspective for Quads: {Module.Toggles.HasFlag(Module._Toggles.Quad) && Module.Toggles.HasFlag(Module._Toggles.Perspective)}";
                if (Module.Toggles.HasFlag(Module._Toggles.Quad))
                {
                    BLANKS[3] = false;
                    PerspectiveQuadMode.FontColor = Font.ColorID.White;
                }
                else
                {
                    BLANKS[3] = true;
                    PerspectiveQuadMode.FontColor = Font.ColorID.Grey;
                }
                ClassicSpriteBatchMode.Data = $"Classic SpriteBatch: {Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch)}";
                BLANKS[4] = false;
                FourceDump.Data = $"Onload Dump Textures: {Module.Toggles.HasFlag(Module._Toggles.DumpingData)}";
                BLANKS[5] = false;
                Deswizzle.Data = $"Deswizzle Tiles";
                BLANKS[6] = false;
                Reswizzle.Data = $"Reswizzle Tiles";
                BLANKS[7] = false;
                foreach (int i in Enumerable.Range(8, Rows-8))
                {
                    ITEM[i, 0].Hide();
                    BLANKS[i] = true;
                }
            }
            else
            {
                foreach (int i in Enumerable.Range(1, Rows))
                {
                    ITEM[i,0].Hide();
                    BLANKS[i] = true;
                }
            }
            BLANKS[Count - 1] = true;
            base.Refresh();
        }

        public override bool Update()
        {
            if ((Module.Background?.MouseLocation ?? Vector3.Zero) != Vector3.Zero)
                MouseLocationIn3D.Data = $"Mouse Cords: {Module.Background?.MouseLocation}";
            else
                MouseLocationIn3D.Data = null;
            return base.Update();
        }

        protected override void Init()
        {
            base.Init();
            foreach (int i in Enumerable.Range(0, Count))
            {
                ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
            }
            Cursor_Status = Cursor_Status.Enabled;
            MouseLocationIn3D = new IGMDataItem.Text { Pos = SIZE[Rows - 1], Scale = new Vector2(1.5f) };
            MouseLocationIn3D.Y = Y + Height + 10;
            skiprefresh = true;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
        }

        #endregion Methods
    }
}