

namespace OpenVIII.Fields
{
    public interface IFormattableScript
    {
        void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);
    }
}