namespace OpenVIII.Fields.Scripts
{
    public interface IScriptFormatterContext
    {
        #region Methods

        string GetMessage(int messageIndex);

        string GetObjectNameByIndex(int index);

        void GetObjectScriptNamesById(int id, out string objectName, out string scriptName);

        #endregion Methods
    }
}