using System;

namespace OpenVIII.Fields
{
    public sealed class InteractionService : IInteractionService
    {
        #region Fields

        private readonly int[] _executionResults = new int[ScriptResultId.MaxIndex + 1];

        #endregion Fields

        #region Properties

        public bool IsSupported => true;

        #endregion Properties

        #region Indexers

        public int this[ScriptResultId id]
        {
            get => _executionResults[id.ResultId];
            set => _executionResults[id.ResultId] = value;
        }

        #endregion Indexers

        #region Methods

        public IAwaitable Wait(int frameNumber)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(InteractionService)}.{nameof(Wait)}({nameof(frameNumber)}: {frameNumber})");
            return DummyAwaitable.Instance;
        }

        #endregion Methods
    }
}