using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenVIII
{
    public class IGMDataItem_ATB_Gradient : IGMDataItem_Texture
    {
        #region Fields

        private float _percent;

        #endregion Fields

        #region Methods

        protected override void Init()
        {
            float dark = 0.067f;
            float fade = 0.933f;
            Restriction = Pos;
            int total = Pos.Width;
            Color lightline = new Color(118, 118, 118, 255);
            Color darkline = new Color(58, 58, 58, 255);
            Color[] cfade = new Color[total];
            int i;
            for (i = 0; i < cfade.Length - (dark * total); i++)
                cfade[i] = Color.Lerp(Color.Black, lightline, i / (fade * total));

            for (; i < cfade.Length; i++)
                cfade[i] = darkline;
            Data = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1);
            Width = Data.Width;
            Data.SetData(cfade);
            base.Init();
            
        }
        bool First = true;
        #endregion Methods

        #region Constructors

        public IGMDataItem_ATB_Gradient(Rectangle? pos = null) : base(null, pos, Color.White, Color.White, 1f) => Init();

        #endregion Constructors

        #region Properties

        public int ATBBarIncrement { get; private set; } = 0;
        public int ATBBarPos { get; private set; } = 0;
        public bool Done => Percent >= 1f;
        public float Percent { get => _percent > 1f ? 1f : _percent; private set => _percent = value; }

        #endregion Properties

        public override void Refresh(Characters character, Characters? Visiblecharacter = null)
        {
            base.Refresh(character, Visiblecharacter);
            if (First)
            {
                ATBBarPos = Memory.State[Character].ATBBarStart();
                First = false;
            }
            else

                ATBBarPos = 0;
        }


        public override bool Update()
        {
            if (Enabled)
            {
                if (!Done && Character != Characters.Blank)
                {
                    double tms = Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                    ATBBarIncrement = Memory.State[Character].BarIncrement(); // per 60fps
                    ATBBarPos += (int)(ATBBarIncrement * tms / 60);
                    Percent = (float)ATBBarPos / Enemy.ATBBarSize;
                    X = Lerp(Restriction.X - Width, Restriction.X, Percent);

                    if (Memory.State[Character].IsDead)
                    {
                        //Color = Faded_Color = Color.Red * .5f;
                        ATBBarPos = 0;
                    }
                    else if (Memory.State[Character].IsPetrify)
                    {
                        Color = Faded_Color = Color.Gray * .8f;
                    }
                    else if ((Memory.State[Character].Statuses1 & Kernel_bin.Battle_Only_Statuses.Stop) != 0)
                    {
                        Color = Faded_Color = Color.DarkBlue * .8f;
                    }
                    else if ((Memory.State[Character].Statuses1 & Kernel_bin.Battle_Only_Statuses.Slow) != 0)
                    {
                        Color = Faded_Color = Color.DarkCyan * .8f;
                    }
                    else if ((Memory.State[Character].Statuses1 & Kernel_bin.Battle_Only_Statuses.Haste) != 0)
                    {
                        Color = Faded_Color = Color.Violet * .8f;
                    }
                    else Color = Faded_Color = Color.Orange * .8f;
                }
                return true;
            }
            return false;
            int Lerp(int x, int y, float p) => (int)Math.Round(MathHelper.Lerp(x, y, p));
        }
    }
}