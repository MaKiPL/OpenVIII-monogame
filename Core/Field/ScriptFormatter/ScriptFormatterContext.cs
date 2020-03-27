using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public sealed class ScriptFormatterContext : IScriptFormatterContext
    {
        #region Fields

        private IReadOnlyList<FF8String> _messages;
        private Sym.GameObjects _symbols;

        #endregion Fields

        #region Methods

        public string GetMessage(int messageIndex)
        {
            if (messageIndex < _messages.Count)
                return _messages[messageIndex];

            return $"INVALID MESSAGE {messageIndex:D3}";
        }

        public string GetObjectNameByIndex(int index)
        {
            var objectName = string.Empty;
            if (_symbols != null)
                objectName = _symbols.GetObjectOrDefault(index, defaultValue: string.Empty).Name;

            if (string.IsNullOrEmpty(objectName))
                return $"ObjectIndex_{index:D2}";

            return objectName;
        }

        public void GetObjectScriptNamesById(int id, out string objectName, out string scriptName)
        {
            if (_symbols != null && _symbols.FindByLabel(id, out var obj, out scriptName))
            {
                objectName = obj.Name;
                return;
            }

            objectName = $"ObjectId_{id:D2}";
            scriptName = $"Script_{id:D2}";
        }

        public void SetMessages(IReadOnlyList<FF8String> messages) => _messages = messages;

        public void SetSymbols(Sym.GameObjects symbols) => _symbols = symbols;

        #endregion Methods
    }
}