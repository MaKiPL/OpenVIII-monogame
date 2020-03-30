using System;

namespace OpenVIII.Fields
{
    public struct GlobalVariableId<T> where T : unmanaged
    {
        #region Fields

        public static bool IsValid = Validate();

        #endregion Fields

        #region Constructors

        public GlobalVariableId(int variableId)
        {
            if (variableId < 0)
                throw new ArgumentOutOfRangeException(nameof(variableId), $"Invalid global variable ID: {variableId}");

            VariableId = variableId;
        }

        #endregion Constructors

        #region Properties

        public static string TypeName { get; } = typeof(T).Name;
        public int VariableId { get; }

        #endregion Properties

        #region Methods

        public static bool Validate()
        {
            unsafe
            {
                if (sizeof(T) > 8)
                    throw new NotSupportedException($"The global variable must be an integer and cannot exceed 8 bytes. {TypeName} isn't supported.");
            }

            return true;
        }

        public override string ToString() => $"Global({TypeName}): {VariableId}";

        #endregion Methods
    }
}