using System;

namespace OpenVIII.Fields
{
    public struct ScriptResultId
    {
        #region Fields

        public const int MaxIndex = 8;

        #endregion Fields

        #region Constructors

        public ScriptResultId(int resultId)
        {
            if (resultId < 0 || resultId > MaxIndex)
                throw new ArgumentOutOfRangeException(nameof(resultId), $"Invalid temporary result variable index: {resultId}");

            ResultId = resultId;
        }

        #endregion Constructors

        #region Properties

        public int ResultId { get; }

        #endregion Properties

        #region Indexers

        public int this[IInteractionService service]
        {
            get => service[this];
            set => service[this] = value;
        }

        #endregion Indexers

        #region Methods

        public override string ToString() => $"ResultId: {ResultId}";

        #endregion Methods
    }
}