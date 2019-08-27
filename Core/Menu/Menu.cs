using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {

        public abstract class Menu
        {
            ///// <summary>
            ///// replace me with new keyword
            ///// </summary>
            //protected enum Mode
            //{
            //    UNDEFINDED
            //}
            public abstract void SetMode(Enum mode);
            public abstract Enum GetMode();

            public bool Enabled { get; private set; } = true;
            public virtual void Hide() => Enabled = false;
            public virtual void Show() => Enabled = true;

            public Dictionary<Enum, IGMData> Data;

            ///// <summary>
            ///// replace me with new keyword or cast me to your new enum.
            ///// </summary>
            //protected Enum mode=(Mode)0;

            private Vector2 _size;
            //static public Vector2 TextScale { get; protected set; }
            static public Matrix Focus { get; protected set; }
            private bool skipdata;

            public Vector2 Size { get => _size; protected set => _size = value; }

            public Menu()
            {
                Data = new Dictionary<Enum, IGMData>();
                Init();
                skipdata = true;
                ReInit();
                skipdata = false;
            }

            protected virtual void Init()
            {
            }

            public virtual void ReInit()
            {
                if(!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.ReInit();
                //Update();
            }

            public virtual void StartDraw()
            {
                if (Enabled)
                    Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: Focus);
            }

            public virtual void Draw()
            {
                StartDraw();
                DrawData();
                EndDraw();
            }
            public virtual void DrawData()
            {
                if (!skipdata && Enabled)
                    foreach (KeyValuePair<Enum, IGMData> i in Data)
                        i.Value.Draw();
            }
            public virtual void EndDraw()
            {
                if(Enabled)
                Memory.SpriteBatchEnd();
            }

            public virtual bool Update()
            {
                bool ret = false;
                Vector2 Zoom = Memory.Scale(Size.X, Size.Y, Memory.ScaleMode.FitBoth);
                Focus = Matrix.CreateTranslation((Size.X / -2), (Size.Y / -2), 0) *
                    Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                    Matrix.CreateTranslation(vp.X / 2, vp.Y / 2, 0);
                if (Enabled)
                {
                    //todo detect when there is no saves detected.
                    //check for null
                    if (!skipdata)
                        foreach (KeyValuePair<Enum, IGMData> i in Data)
                        {
                            ret = i.Value.Update() || ret;
                        }
                }
                return Inputs() || ret;
            }            

            protected abstract bool Inputs();
        }
    }
}