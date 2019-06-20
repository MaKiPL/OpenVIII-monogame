

namespace FF8
{
    public interface IFormattableScript
    {
        void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services);
    }
}