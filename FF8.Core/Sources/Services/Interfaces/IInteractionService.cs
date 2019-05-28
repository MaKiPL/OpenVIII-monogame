using System;

namespace FF8.Core
{
    public interface IInteractionService
    {
        Boolean IsSupported { get; }

        Int32 this[ScriptResultId id] { get; set; }
 
        IAwaitable Wait(Int32 frameNumber);
    }
}