using System;

namespace OpenVIII.Fields.Scripts
{
    public interface IConstExpression : ILogicalExpression
    {
        #region Properties

        Int64 Value { get; }

        #endregion Properties
    }

    public interface IJsmExpression : IFormattableScript
    {
        #region Methods

        Int64 Calculate(IServices services);

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

        public static Boolean Boolean(this IJsmExpression expression, IServices services)
        {
            Int32 value = Int32(expression, services);
            if (value == 0)
                return false;
            if (value == 1)
                return true;

            throw new NotSupportedException($"Cannot convert value {value} to Boolean.");
        }

        public static Boolean Boolean(this IConstExpression expression)
        {
            if (expression.Value == 0)
                return false;
            if (expression.Value == 1)
                return true;

            throw new NotSupportedException($"Cannot convert value {expression.Value} to Boolean.");
        }

        public static Characters Characters(this IJsmExpression expression, IServices services) => checked((Characters)expression.Calculate(services));

        public static Characters Characters(this IConstExpression expression) => checked((Characters)expression.Value);

        public static GFs GFs(this IJsmExpression expression, IServices services) => checked((GFs)expression.Calculate(services));

        public static GFs GFs(this IConstExpression expression) => checked((GFs)expression.Value);

        public static Cards.ID Cards(this IJsmExpression expression, IServices services) => checked((Cards.ID)expression.Calculate(services));

        public static Cards.ID Cards(this IConstExpression expression) => checked((Cards.ID)expression.Value);

        public static Byte Byte(this IJsmExpression expression, IServices services) => checked((Byte)expression.Calculate(services));

        public static Byte Byte(this IConstExpression expression) => checked((Byte)expression.Value);

        public static UInt16 UInt16(this IJsmExpression expression, IServices services) => checked((UInt16)expression.Calculate(services));

        public static UInt16 UInt16(this IConstExpression expression) => checked((UInt16)expression.Value);

        public static Int16 Int16(this IJsmExpression expression, IServices services) => checked((Int16)expression.Calculate(services));

        public static Int16 Int16(this IConstExpression expression) => checked((Int16)expression.Value);

        public static Int32 Int32(this IJsmExpression expression, IServices services) => checked((Int32)expression.Calculate(services));

        public static Int32 Int32(this IConstExpression expression) => checked((Int32)expression.Value);

        public static ILogicalExpression LogicalEvaluate(this ILogicalExpression expression, IServices services)
        {
            IJsmExpression jsm = expression.Evaluate(services);
            return (ILogicalExpression)jsm;
        }

        #endregion Methods
    }
}