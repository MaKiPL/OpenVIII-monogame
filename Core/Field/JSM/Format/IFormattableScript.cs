namespace OpenVIII.Fields.Scripts
{
    public interface IFormattableScript
    {
        #region Methods

        void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);

        #endregion Methods
    }
}