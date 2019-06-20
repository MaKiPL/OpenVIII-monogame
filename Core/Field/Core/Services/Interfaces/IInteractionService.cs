using System;

namespace FF8
{
    public interface IInteractionService
    {
        Boolean IsSupported { get; }

        Int32 this[ScriptResultId id] { get; set; }
 
        IAwaitable Wait(Int32 frameNumber);
    }
}