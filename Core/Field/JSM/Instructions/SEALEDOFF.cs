using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Enable Sealed Options (Ultimecia's Castle)</para>
    /// <para>SEALEDOFF? on testbl9 only.</para>
    /// <para>Enables features of the game pertaining to the last dungeon's mechanic (items, saving, etc).</para>
    /// <para>Whether or not these are enabled/disabled is stored in byte 334. 0=Sealed, 1=Unsealed</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/159_SEALEDOFF"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/Variables"/>
    public sealed class SEALEDOFF : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// If flag is set then an option is enabled.
        /// </summary>
        private readonly SFlags _flags;

        #endregion Fields

        #region Constructors

        public SEALEDOFF(SFlags flags) => _flags = flags;

        public SEALEDOFF(int parameter, IStack<IJsmExpression> stack)
            : this(
                flags: (SFlags)((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Enums

        [Flags]
        public enum SFlags : byte
        {
            AllDisabled = 0x0, //may not be a real value?
            Items = 0x1, //1. Items
            Magic = 0x2, //2. Magic
            GF = 0x4, //4. GF
            Draw = 0x8, //8. Draw
            CommandAbility = 0x10, //16. Command Ability
            LimitBreak = 0x20, //32. Limit Break
            Resurrection = 0x40, //64. Resurrection
            Save = 0x80, //128. Save
        }

        #endregion Enums

        #region Methods

        public override string ToString() => $"{nameof(SEALEDOFF)}({nameof(_flags)}: {_flags})";

        #endregion Methods
    }
}