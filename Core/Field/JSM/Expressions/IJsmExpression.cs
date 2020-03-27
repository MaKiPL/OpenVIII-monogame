using System;

namespace OpenVIII.Fields.Scripts
{
    public interface IConstExpression : ILogicalExpression
    {
        #region Properties

        long Value { get; }

        #endregion Properties
    }

    public interface IJsmExpression : IFormattableScript
    {
        #region Methods

        long Calculate(IServices services);

        IJsmExpression Evaluate(IServices services);

        #endregion Methods
    }

    public interface ILogicalExpression : IJsmExpression
    {
        #region Methods

        ILogicalExpression LogicalInverse();

        #endregion Methods
    }

    public static class ExtensionMethods
    {
        #region Methods

        public static bool Boolean(this IJsmExpression expression, IServices services)
        {
            var value = Int32(expression, services);
            switch (value)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new NotSupportedException($"Cannot convert value {value} to Boolean.");
            }
        }

        public static bool Boolean(this IConstExpression expression)
        {
            switch (expression.Value)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    throw new NotSupportedException($"Cannot convert value {expression.Value} to Boolean.");
            }
        }

        public static byte Byte(this IJsmExpression expression, IServices services) => checked((byte)expression.Calculate(services));

        public static byte Byte(this IConstExpression expression) => checked((byte)expression.Value);

        public static Cards.ID Cards(this IJsmExpression expression, IServices services) => checked((Cards.ID)expression.Calculate(services));

        public static Cards.ID Cards(this IConstExpression expression) => checked((Cards.ID)expression.Value);

        public static Characters Characters(this IJsmExpression expression, IServices services) => checked((Characters)expression.Calculate(services));

        public static Characters Characters(this IConstExpression expression) => checked((Characters)expression.Value);

        public static GFs GFs(this IJsmExpression expression, IServices services) => checked((GFs)expression.Calculate(services));

        public static GFs GFs(this IConstExpression expression) => checked((GFs)expression.Value);

        public static short Int16(this IJsmExpression expression, IServices services) => checked((short)expression.Calculate(services));

        public static short Int16(this IConstExpression expression) => checked((short)expression.Value);

        public static int Int32(this IJsmExpression expression, IServices services) => checked((int)expression.Calculate(services));

        public static int Int32(this IConstExpression expression) => checked((int)expression.Value);

        public static ILogicalExpression LogicalEvaluate(this ILogicalExpression expression, IServices services)
        {
            var jsm = expression.Evaluate(services);
            return (ILogicalExpression)jsm;
        }

        public static ushort UInt16(this IJsmExpression expression, IServices services) => checked((ushort)expression.Calculate(services));

        public static ushort UInt16(this IConstExpression expression) => checked((ushort)expression.Value);

        #endregion Methods
    }
}