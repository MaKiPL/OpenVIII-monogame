namespace OpenVIII
{
    /// <summary>
    /// Root class all menu objects can grow from.
    /// </summary>
    public abstract class Menu_Base
    {
        #region Properties

        /// <summary>
        /// If enabled the menu is visable and all functionality works. Else everything is hidden and
        /// nothing functions.
        /// </summary>
        public bool Enabled { get; private set; } = true;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Hide object prevents drawing, update, inputs.
        /// </summary>
        public virtual void Hide() => Enabled = false;

        public abstract bool Inputs();

        public abstract void Refresh();

        /// <summary>
        /// Show object enables drawing, update, inputs.
        /// </summary>
        public virtual void Show() => Enabled = true;

        public abstract bool Update();

        protected abstract void Init();

        #endregion Methods
    }
}