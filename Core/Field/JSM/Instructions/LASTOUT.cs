using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Last dungeon out</para>
    /// <para>Ends the effects of LASTIN.</para>
    /// <para>fehall1</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/158_LASTOUT"/>
    public sealed class LASTOUT : JsmInstruction
    {
        #region Constructors

        public LASTOUT()
        {
        }

        public LASTOUT(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override String ToString() => $"{nameof(LASTOUT)}()";

        #endregion Methods
    }
}