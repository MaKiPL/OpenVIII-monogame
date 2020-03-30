using OpenVIII.Fields.Scripts.Instructions;
using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Control
        {
            #region Classes

            public sealed partial class While
            {
                #region Classes

                private sealed class Executor : IScriptExecutor
                {
                    #region Fields

                    private readonly WhileSegment _seg;

                    #endregion Fields

                    #region Constructors

                    public Executor(WhileSegment seg) => _seg = seg;

                    #endregion Constructors

                    #region Methods

                    public IEnumerable<IAwaitable> Execute(IServices services)
                    {
                        while (CanExecute(_seg.Jpf, services))
                        {
                            var executable = _seg.GetBodyInstructions();
                            var executer = ExecutableSegment.GetExecuter(executable);
                            foreach (var result in executer.Execute(services))
                                yield return result;

                            // Skip one iteration to give control to other operations.
                            yield return SpinAwaitable.Instance;
                        }
                    }

                    private bool CanExecute(JPF jpf, IServices services)
                    {
                        foreach (var condition in jpf.Conditions)
                        {
                            if (!condition.Boolean(services))
                                return false;
                        }

                        return true;
                    }

                    #endregion Methods
                }

                #endregion Classes
            }

            #endregion Classes
        }

        #endregion Classes
    }
}