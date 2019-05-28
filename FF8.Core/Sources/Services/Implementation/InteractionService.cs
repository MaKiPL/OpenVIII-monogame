using System;

namespace FF8.Core
{
    public sealed class InteractionService : IInteractionService
    {
        public Boolean IsSupported => true;

        private readonly Int32[] _executionResults = new Int32[ScriptResultId.MaxIndex + 1];

        public Int32 this[ScriptResultId id]
        {
            get => _executionResults[id.ResultId];
            set => _executionResults[id.ResultId] = value;
        }

        public IAwaitable Wait(Int32 frameNumber)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(InteractionService)}.{nameof(Wait)}({nameof(frameNumber)}: {frameNumber})");
            return DummyAwaitable.Instance;
        }
    }
}