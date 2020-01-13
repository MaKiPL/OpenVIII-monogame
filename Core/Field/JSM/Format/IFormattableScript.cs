

namespace OpenVIII.Fields.Scripts
{
    public interface IFormattableScript
    {
        void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);
    }
}