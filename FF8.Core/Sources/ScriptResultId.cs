using System;

namespace FF8.Core
{
    public struct ScriptResultId
    {
        public const Int32 MaxIndex = 8;

        public Int32 ResultId { get; }

        public ScriptResultId(Int32 resultId)
        {
            if (resultId < 0 || resultId > MaxIndex)
                throw new ArgumentOutOfRangeException(nameof(resultId), $"Invalid temporary result variable index: {resultId}");

            ResultId = resultId;
        }

        public override String ToString()
        {
            return $"ResultId: {ResultId}";
        }

        public Int32 this[IInteractionService service]
        {
            get => service[this];
            set => service[this] = value;
        }
    }
}