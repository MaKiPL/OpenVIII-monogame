using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Pauses execution of this script until the current FMV movie is finished playing. 
    /// </summary>
    internal sealed class MOVIESYNC : JsmInstruction
    {
        public MOVIESYNC()
        {
        }

        public MOVIESYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVIESYNC)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IMovieService))
                .Method(nameof(IMovieService.Wait))
                .Comment(nameof(MOVIE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Movie[services].Wait();
            return DummyAwaitable.Instance;
        }
    }
}