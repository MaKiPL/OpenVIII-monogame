using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Root class all menu objects can grow from.
    /// </summary>
    public abstract class Menu_Base
    {
        #region Properties

        /// <summary>
        /// If enabled the menu is Visible and all functionality works. Else everything is hidden and
        /// nothing functions.
        /// </summary>
        public bool Enabled { get; private set; } = true;

        /// <summary>
        /// Characters/Enemies/GF
        /// </summary>
        public Damageable Damageable { get; protected set; }



        /// <summary>
        /// Position of party member 0,1,2. If -1 at the time of setting the character wasn't in the party.
        /// </summary>
        public sbyte PartyPos { get; protected set; }

        #endregion Properties

        #region Methods

        public abstract void Draw();

        /// <summary>
        /// Hide object prevents drawing, update, inputs.
        /// </summary>
        public virtual void Hide() => Enabled = false;
        public virtual void HideChildren() { }

        public abstract bool Inputs();

        /// <summary>
        /// Things that change rarely. Like a party member changes or Laguna dream happens.
        /// </summary>
        public virtual void Refresh() => RefreshChild();

        /// <summary>
        /// For child items.
        /// </summary>
        protected virtual void RefreshChild()
        {
        }

        /// <summary>
        /// Update set characters and then refresh.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="Visiblecharacter"></param>
        public virtual void Refresh(Damageable damageable)
        {
            if(damageable != null )
            {
                Damageable = damageable;

                if (Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    PartyPos = (sbyte)(Memory.State?.PartyData?.Where(x => !x.Equals(Characters.Blank)).ToList().FindIndex(x => x.Equals(c.ID)) ?? -1);
                }
            }
            Refresh();
        }

        /// <summary>
        /// Plan is to use this to reset values to a default state if done.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Show object enables drawing, update, inputs.
        /// </summary>
        public virtual void Show() => Enabled = true;

        public abstract bool Update();

        protected abstract void Init();

        #endregion Methods
    }
}