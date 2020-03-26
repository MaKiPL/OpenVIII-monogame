using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.Fields.IGMData
{
    public class FieldDebugControls : OpenVIII.IGMData.Base
    {
        #region Fields

        private const int TotalRows = 8;
        private const int TotalBelow = 2;
        private bool _skipRefresh;

        #endregion Fields

        #region Properties

        public IGMDataItem.Text ClassicSpriteBatchMode { get => (IGMDataItem.Text)ITEM[4, 0]; protected set => ITEM[4, 0] = value; }
        public IGMDataItem.Text Deswizzle { get => (IGMDataItem.Text)ITEM[6, 0]; protected set => ITEM[6, 0] = value; }
        public IGMDataItem.Text FieldName { get => (IGMDataItem.Text)ITEM[0, 0]; protected set => ITEM[0, 0] = value; }
        public IGMDataItem.Text ForceDump { get => (IGMDataItem.Text)ITEM[5, 0]; protected set => ITEM[5, 0] = value; }
        public IGMDataItem.Text MouseLocationIn3D { get => (IGMDataItem.Text)ITEM[Count - 2, 0]; protected set => ITEM[Count - 2, 0] = value; }
        public IGMDataItem.Text AreaName { get => (IGMDataItem.Text)ITEM[Count - 1, 0]; protected set => ITEM[Count - 1, 0] = value; }
        public IGMDataItem.Text PerspectiveQuadMode { get => (IGMDataItem.Text)ITEM[3, 0]; protected set => ITEM[3, 0] = value; }
        public IGMDataItem.Text QuadBG { get => (IGMDataItem.Text)ITEM[2, 0]; protected set => ITEM[2, 0] = value; }
        public IGMDataItem.Text Reswizzle { get => (IGMDataItem.Text)ITEM[7, 0]; protected set => ITEM[7, 0] = value; }
        public IGMDataItem.Text WalkMesh { get => (IGMDataItem.Text)ITEM[1, 0]; protected set => ITEM[1, 0] = value; }

        #endregion Properties

        #region Methods

        public static FieldDebugControls Create(Rectangle pos) => Create<FieldDebugControls>(TotalRows + TotalBelow, 1, new IGMDataItem.Box { Pos = pos }, 1, TotalRows);

        public override bool Inputs()
        {
            Memory.IsMouseVisible = true;
            if (Input2.DelayedButton(MouseButtons.MiddleButton))
            {
                Debug.WriteLine($"=== Tiles Under MouseLocation: {Module.Background.MouseLocation} ===");
                foreach (var tile in Module.Background.TilesUnderMouse())
                {
                    Debug.WriteLine(tile);
                }
                return true;
            }

            if (!Input2.DelayedButton(Keys.F5)) return base.Inputs();
            SetCursor_select(0);
            Inputs_OKAY();
            return true;
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
            var i = 0;
            if (CURSOR_SELECT == i++)
            {
                Module.Background?.Dispose();//force all textures to reload.
                Module.ResetField();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Toggles.WalkMesh);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Toggles.Quad);
                if (Module.Toggles.HasFlag(Toggles.ClassicSpriteBatch))
                    Module.Toggles = Module.Toggles.Flip(Toggles.ClassicSpriteBatch);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                if (Module.Toggles.HasFlag(Toggles.Quad))
                {
                    Module.Toggles = Module.Toggles.Flip(Toggles.Perspective);
                    Refresh();
                }
                else skipsnd = true;
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Toggles.ClassicSpriteBatch);
                if (Module.Toggles.HasFlag(Toggles.Quad))
                    Module.Toggles = Module.Toggles.Flip(Toggles.Quad);
                if (Module.Background.HasSpriteBatchTexturesLoaded)
                    Refresh();
                else
                    Module.ResetField();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Toggles = Module.Toggles.Flip(Toggles.DumpingData);
                Refresh();
            }
            else if (CURSOR_SELECT == i++)
            {
                Module.Background.Deswizzle();
                Refresh();
            }
            else if (CURSOR_SELECT == i)
            {
                Module.Background.Reswizzle();
                Refresh();
            }
            else skipsnd = true;

            base.Inputs_OKAY();
            return true;
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
            if (_skipRefresh)
            {
                _skipRefresh = false;
                return;
            }
            FieldName.Data = $"Field: { Memory.FieldHolder.FieldID} - { Memory.FieldHolder.GetString()?.ToUpper()}";

            BLANKS[0] = false;
            if (Module.Mod != FieldModes.Disabled)
            {
                WalkMesh.Data = $"Draw WalkMesh: {Module.Toggles.HasFlag(Toggles.WalkMesh)}";
                QuadBG.Data = $"Draw Quad BG: {Module.Toggles.HasFlag(Toggles.Quad)}";

                PerspectiveQuadMode.Data = $"Perspective for Quads: {Module.Toggles.HasFlag(Toggles.Quad) && Module.Toggles.HasFlag(Toggles.Perspective)}";
                if (Module.Toggles.HasFlag(Toggles.Quad))
                {
                    BLANKS[3] = false;
                    PerspectiveQuadMode.FontColor = Font.ColorID.White;
                }
                else
                {
                    BLANKS[3] = true;
                    PerspectiveQuadMode.FontColor = Font.ColorID.Grey;
                }
                ClassicSpriteBatchMode.Data = $"Classic SpriteBatch: {Module.Toggles.HasFlag(Toggles.ClassicSpriteBatch)}";
                ForceDump.Data = $"Onload Dump Textures: {Module.Toggles.HasFlag(Toggles.DumpingData)}";
                Deswizzle.Data = "Deswizzle Tiles";
                Reswizzle.Data = "Reswizzle Tiles";
                foreach (var i in Enumerable.Range(1, Rows))
                {
                    ITEM[i, 0].Show();
                    if (i != 3)
                        BLANKS[i] = false;
                }
                foreach (var i in Enumerable.Range(8, Rows - 8))
                {
                    ITEM[i, 0].Hide();
                    BLANKS[i] = true;
                }
            }
            else
            {
                foreach (var i in Enumerable.Range(1, Rows))
                {
                    ITEM[i, 0].Hide();
                    BLANKS[i] = true;
                }
            }
            BLANKS[Count - 1] = true;
            BLANKS[Count - 2] = true;
            AreaName.Data = Module.AreaName;
            base.Refresh();
        }

        public override bool Update()
        {
            MouseLocationIn3D.Data = (Module.Background?.MouseLocation ?? Vector3.Zero) != Vector3.Zero
                ? $"Mouse Cords: {Module.Background?.MouseLocation}"
                : null;
            return base.Update();
        }

        protected override void Init()
        {
            base.Init();
            foreach (var i in Enumerable.Range(0, Count))
            {
                ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
            }
            Cursor_Status = Cursor_Status.Enabled;
            AreaName.Pos=
            MouseLocationIn3D.Pos = SIZE[Rows - 1];
            AreaName.Scale =
            MouseLocationIn3D.Scale = new Vector2(1.5f);
            AreaName.Y =
            MouseLocationIn3D.Y = Y + Height + 10;
            AreaName.Y += 16;
            _skipRefresh = true;
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