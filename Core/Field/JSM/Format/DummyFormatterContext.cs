namespace OpenVIII.Fields.Scripts
{
    public sealed class DummyFormatterContext : IScriptFormatterContext
    {
        #region Properties

        public static IScriptFormatterContext Instance { get; } = new DummyFormatterContext();

        #endregion Properties

        #region Methods

        public string GetMessage(int messageIndex) => $"Message_{messageIndex:D3}";

        public string GetObjectNameByIndex(int index) => $"ObjectIndex_{index:D2}";

        public void GetObjectScriptNamesById(int id, out string objectName, out string scriptName)
        {
            objectName = $"ObjectId_{id:D2}";
            scriptName = $"Script_{id:D2}";
        }

        #endregion Methods
    }
}