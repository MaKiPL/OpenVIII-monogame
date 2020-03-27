namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Unknown. All footstep calls from the game use the following sets of parameters-arguments: 0-0, 0-1, 1-0, 1-2, 2-3, 3-4, 4-5, 5-6, 6-7, 8-9, 10-11</para>
    /// </summary>
    public sealed class FOOTSTEP : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;
        private readonly int _parameter;

        #endregion Fields

        #region Constructors

        public FOOTSTEP(int parameter, IJsmExpression arg0)
        {
            _parameter = parameter;
            _arg0 = arg0;
        }

        public FOOTSTEP(int parameter, IStack<IJsmExpression> stack)
            : this(parameter,
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(FOOTSTEP)}({nameof(_parameter)}: {_parameter}, {nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}