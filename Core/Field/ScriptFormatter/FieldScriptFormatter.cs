using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Fields.Scripts
{
    public static class FieldScriptFormatter
    {
        #region Methods

        public static IEnumerable<FormattedObject> FormatAllObjects(Field.ILookupService lookupService)
        {
            return lookupService.EnumerateAll().SelectMany(FormatFieldObjects);
        }

        public static IEnumerable<FormattedObject> FormatFieldObjects(Field.Info field)
        {
            if (!field.TryReadData(Field.Part.Jsm, out var jsmData))
                yield break;

            var gameObjects = Jsm.File.Read(jsmData);

            if (gameObjects.Count == 0)
                yield break;

            IScriptFormatterContext formatterContext = GetFormatterContext(field);
            var executionContext = StatelessServices.Instance;
            var sw = new ScriptWriter();

            foreach (var obj in gameObjects)
            {
                formatterContext.GetObjectScriptNamesById(obj.Id, out var objectName, out _);
                var formattedScript = FormatObject(obj, sw, formatterContext, executionContext);
                yield return new FormattedObject(field, objectName, formattedScript);
            }
        }

        private static string FormatObject(Jsm.GameObject gameObject, ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
        {
            gameObject.FormatType(sw, formatterContext, executionContext);
            return sw.Release();
        }

        private static ScriptFormatterContext GetFormatterContext(Field.Info field)
        {
            var context = new ScriptFormatterContext();

            if (field.TryReadData(Field.Part.Sym, out var symData))
                context.SetSymbols(Sym.Reader.FromBytes(symData));

            if (field.TryReadData(Field.Part.Msd, out var msdData))
                context.SetMessages(Msd.Reader.FromBytes(msdData));
            return context;
        }

        #endregion Methods
    }
}