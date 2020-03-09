using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Set Area Display Name</para>
    /// <para>This controls what shows up at the bottom of the menu and in your saved game slots.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/133_SETPLACE"/>
    public sealed class SETPLACE : JsmInstruction
    {
        #region Fields

        private readonly Int32 _areaId;

        #endregion Fields

        #region Constructors

        public SETPLACE(Int32 areaId) => _areaId = areaId;

        public SETPLACE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                areaId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Properties

        public int AreaId => _areaId;

        #endregion Properties

        #region Methods

        public FF8String AreaName()
        {
            FF8StringReference s = Memory.Strings[Strings.FileID.AreaNames][0, _areaId];
            if (s.Length > 0)
                return s;
            return null;
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .CommentLine(AreaName())
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.BindArea))
                .Argument("areaId", _areaId)
                .Comment(nameof(SETPLACE));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].BindArea(_areaId);
            return DummyAwaitable.Instance;
        }

        public override String ToString() => $"{nameof(SETPLACE)}({nameof(_areaId)}: {_areaId}, {nameof(AreaName)}: {AreaName()})";

        #endregion Methods
    }
}