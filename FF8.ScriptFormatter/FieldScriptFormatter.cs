using System;
using System.Collections.Generic;
using FF8.Core;
using FF8.Encoding;
using FF8.Fields;
using FF8.JSM.Format;
using FF8.MSD;
using FF8.SYM;
using Jsm = FF8.JSM.Jsm;

namespace FF8.ScriptFormatter
{
    public static class FieldScriptFormatter
    {
        public static IEnumerable<FormattedObject> FormatAllObjects(Field.ILookupService lookupService)
        {
            foreach (Field.Info field in lookupService.EnumerateAll())
            foreach (FormattedObject formattedObject in FormatFieldObjects(field))
                yield return formattedObject;
        }

        public static IEnumerable<FormattedObject> FormatFieldObjects(Field.Info field)
        {
            if (!field.TryReadData(Field.Part.Jsm, out var jsmData))
                yield break;

            List<Jsm.GameObject> gameObjects = Jsm.File.Read(jsmData);

            if (gameObjects.Count == 0)
                yield break;

            IScriptFormatterContext formatterContext = GetFormatterContext(field);
            IServices executionContext = StatelessServices.Instance;
            ScriptWriter sw = new ScriptWriter();

            foreach (var obj in gameObjects)
            {
                formatterContext.GetObjectScriptNamesById(obj.Id, out String objectName, out _);
                String formattedScript = FormatObject(obj, sw, formatterContext, executionContext);
                yield return new FormattedObject(field, objectName, formattedScript);
            }
        }

        private static ScriptFormatterContext GetFormatterContext(Field.Info field)
        {
            ScriptFormatterContext context = new ScriptFormatterContext();

            if (field.TryReadData(Field.Part.Sym, out var symData))
                context.SetSymbols(Sym.Reader.FromBytes(symData));

            if (field.TryReadData(Field.Part.Msd, out var msdData))
                context.SetMessages(Msd.Reader.FromBytes(msdData, FF8TextEncoding.Default));
            return context;
        }

        private static String FormatObject(Jsm.GameObject gameObject, ScriptWriter sw, IScriptFormatterContext formatterContext, IServices executionContext)
        {
            gameObject.FormatType(sw, formatterContext, executionContext);
            return sw.Release();
        }
    }
}