using System;
using System.Globalization;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            public sealed class ValueExpression : IConstExpression
            {
                #region Fields

                private readonly TypeCode _typeCode;

                #endregion Fields

                #region Constructors

                private ValueExpression(long value, TypeCode typeCode)
                {
                    Value = value;
                    _typeCode = typeCode;

                    if (_typeCode < TypeCode.SByte || typeCode > TypeCode.UInt32)
                        throw new ArgumentOutOfRangeException($"Type {typeCode} isn't supported.", nameof(typeCode));
                }

                #endregion Constructors

                #region Properties

                public long Value { get; }

                #endregion Properties

                #region Methods

                public static ValueExpression Create(int value) => new ValueExpression(value, TypeCode.Int32);

                public static ValueExpression Create(uint value) => new ValueExpression(value, TypeCode.UInt32);

                public static ValueExpression Create(short value) => new ValueExpression(value, TypeCode.Int16);

                public static ValueExpression Create(ushort value) => new ValueExpression(value, TypeCode.UInt16);

                public static ValueExpression Create(byte value) => new ValueExpression(value, TypeCode.Byte);

                public static ValueExpression Create(sbyte value) => new ValueExpression(value, TypeCode.SByte);

                public long Calculate(IServices services) => Value;

                public IJsmExpression Evaluate(IServices services) => this;

                public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                {
                    switch (_typeCode)
                    {
                        case TypeCode.SByte:
                            sw.Append("(SByte)");
                            break;

                        case TypeCode.Byte:
                            sw.Append("(Byte)");
                            break;

                        case TypeCode.Int16:
                            sw.Append("(Int16)");
                            break;

                        case TypeCode.UInt16:
                            sw.Append("(UInt16)");
                            break;
                    }

                    sw.Append(Value.ToString(CultureInfo.InvariantCulture));

                    if (_typeCode == TypeCode.UInt32)
                        sw.Append("u");
                }

                public ILogicalExpression LogicalInverse() => new ValueExpression(Value == 0 ? 1 : 0, TypeCode.Int32);

                public override string ToString()
                {
                    var sw = new ScriptWriter(capacity: 16);
                    Format(sw, DummyFormatterContext.Instance, StatelessServices.Instance);
                    return sw.Release();
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}