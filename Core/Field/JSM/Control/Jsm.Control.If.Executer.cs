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

            public sealed partial class If
            {
                #region Classes

                private sealed class Executor : IScriptExecutor
                {
                    #region Fields

                    private readonly If _aggregator;

                    #endregion Fields

                    #region Constructors

                    public Executor(If aggregator) => _aggregator = aggregator;

                    #endregion Constructors

                    #region Methods

                    public bool CanExecute(JPF jpf, IServices services)
                    {
                        foreach (var condition in jpf.Conditions)
                        {
                            if (!condition.Boolean(services))
                                return false;
                        }

                        return true;
                    }

                    public IEnumerable<IAwaitable> Execute(IServices services)
                    {
                        var executable = GetExecutableInstructions(services);
                        if (executable == null)
                            yield break;

                        var executer = ExecutableSegment.GetExecuter(executable);
                        foreach (var result in executer.Execute(services))
                            yield return result;
                    }

                    private IEnumerable<IJsmInstruction> GetExecutableInstructions(IServices services)
                    {
                        if (CanExecute(_aggregator.IfRange.Jpf, services))
                            return _aggregator.IfRange.GetBodyInstructions();

                        foreach (var elseIf in _aggregator.ElseIfRanges)
                        {
                            if (CanExecute(elseIf.Jpf, services))
                            {
                                return elseIf.GetBodyInstructions();
                            }
                        }

                        return _aggregator.ElseRange?.GetBodyInstructions();
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