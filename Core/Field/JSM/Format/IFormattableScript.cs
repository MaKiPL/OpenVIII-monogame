

namespace OpenVIII
{
    public interface IFormattableScript
    {
        void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);
    }
}