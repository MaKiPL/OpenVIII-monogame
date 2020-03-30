using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class Expression
        {
            #region Classes

            /// <summary>
            /// <para>Calculate</para>
            /// <para>Calculate value1 Argument value2 and push the result into the stack.</para>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/001_CAL"/>
            public static class CAL
            {
                #region Enums

                public enum Operation
                {
                    // Unary
                    Minus = 5,

                    BitInverse = 15,
                    LogNot = 0x1000002,

                    // Binary
                    Add = 0,

                    Sub = 1,
                    Mul = 2,
                    Mod = 3,
                    Div = 4,
                    Eq = 6,
                    Great = 7,
                    GreatOrEquals = 8,
                    Less = 9,
                    LessOrEquals = 10,
                    NotEquals = 11,
                    And = 12,
                    Or = 13,
                    Xor = 14,
                    LogAnd = 0x1000000,
                    LogOr = 0x1000001
                };

                #endregion Enums

                #region Methods

                public static IJsmExpression Read(int typeValue, IStack<IJsmExpression> stack)
                {
                    var type = (Operation)typeValue;

                    var last = stack.Pop();
                    switch (type)
                    {
                        case Operation.Minus:
                            return new Minus(last);

                        case Operation.BitInverse:
                            return new BitInverse(last);

                        case Operation.LogNot:
                            return new LogNot((ILogicalExpression)last);

                        case Operation.Add:
                            return new Add(stack.Pop(), last);

                        case Operation.Sub:
                            return new Sub(stack.Pop(), last);

                        case Operation.Mul:
                            return new Mul(stack.Pop(), last);

                        case Operation.Mod:
                            return new Mod(stack.Pop(), last);

                        case Operation.Div:
                            return new Div(stack.Pop(), last);

                        case Operation.Eq:
                            return new Eq(stack.Pop(), last);

                        case Operation.Great:
                            return new Great(stack.Pop(), last);

                        case Operation.GreatOrEquals:
                            return new GreatOrEquals(stack.Pop(), last);

                        case Operation.Less:
                            return new Less(stack.Pop(), last);

                        case Operation.LessOrEquals:
                            return new LessOrEquals(stack.Pop(), last);

                        case Operation.NotEquals:
                            return new NotEquals(stack.Pop(), last);

                        case Operation.And:
                            return new BitAnd(stack.Pop(), last);

                        case Operation.Or:
                            return new BitOr(stack.Pop(), last);

                        case Operation.Xor:
                            return new BitXor(stack.Pop(), last);

                        case Operation.LogAnd:
                            return new LogAnd((ILogicalExpression)stack.Pop(), (ILogicalExpression)last);

                        case Operation.LogOr:
                            return new LogOr((ILogicalExpression)stack.Pop(), (ILogicalExpression)last);

                        default:
                            throw new NotSupportedException(type.ToString());
                    }
                }

                #endregion Methods

                #region Classes

                public class Add : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Add(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) + _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value + n2.Value));
                        return new BitAnd(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" + ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} + {_last})";

                    #endregion Methods
                }

                public class BitAnd : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public BitAnd(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) & _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value & n2.Value));
                        return new BitAnd(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" & ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} & {_last})";

                    #endregion Methods
                }

                public class BitInverse : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _value;

                    #endregion Fields

                    #region Constructors

                    public BitInverse(IJsmExpression value) => _value = value;

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => ~(_value.Calculate(services));

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var result = _value.Evaluate(services);
                        if (result is IConstExpression number)
                            return ValueExpression.Create((int)~number.Value);
                        return result;
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("~");
                        _value.Format(sw, formatterContext, services);
                    }

                    public override string ToString() => $"Eval(~{_value})";

                    #endregion Methods
                }

                public class BitOr : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public BitOr(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) | _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value | n2.Value));
                        return new BitOr(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" | ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} | {_last})";

                    #endregion Methods
                }

                public class BitXor : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public BitXor(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) ^ _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value ^ n2.Value));
                        return new BitXor(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" ^ ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} ^ {_last})";

                    #endregion Methods
                }

                public class Div : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Div(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) / _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value / n2.Value));
                        return new Div(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" / ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} / {_last})";

                    #endregion Methods
                }

                public class Eq : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Eq(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) == _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value == n2.Value ? 1 : 0);

                        return new Eq(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" == ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new NotEquals(_first, _last);

                    public override string ToString() => $"Eval({_first} == {_last})";

                    #endregion Methods
                }

                public class Great : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Great(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) > _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value > n2.Value ? 1 : 0);
                        return new Great(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" > ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new LessOrEquals(_first, _last);

                    public override string ToString() => $"Eval({_first} > {_last})";

                    #endregion Methods
                }

                public class GreatOrEquals : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public GreatOrEquals(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) >= _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value >= n2.Value ? 1 : 0);
                        return new GreatOrEquals(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" >= ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new Less(_first, _last);

                    public override string ToString() => $"Eval({_first} >= {_last})";

                    #endregion Methods
                }

                public class Less : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Less(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) < _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value < n2.Value ? 1 : 0);
                        return new Less(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" < ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new GreatOrEquals(_first, _last);

                    public override string ToString() => $"Eval({_first} < {_last})";

                    #endregion Methods
                }

                public class LessOrEquals : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public LessOrEquals(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) <= _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value <= n2.Value ? 1 : 0);
                        return new LessOrEquals(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" <= ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new Great(_first, _last);

                    public override string ToString() => $"Eval({_first} <= {_last})";

                    #endregion Methods
                }

                public class LogAnd : ILogicalExpression
                {
                    #region Fields

                    private readonly ILogicalExpression _first;
                    private readonly ILogicalExpression _last;

                    #endregion Fields

                    #region Constructors

                    public LogAnd(ILogicalExpression first, ILogicalExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => (_first.Calculate(services) != 0 && _last.Calculate(services) != 0) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.LogicalEvaluate(services);
                        var last = _last.LogicalEvaluate(services);
                        if (first is IConstExpression n1)
                        {
                            if (n1.Value == 0)
                            {
                                // (false && ??) => false
                                return ValueExpression.Create(1);
                            }

                            if (last is IConstExpression n2)
                            {
                                if (n2.Value == 0)
                                {
                                    // (true && false) => false
                                    return ValueExpression.Create(1);
                                }
                                else
                                {
                                    // (true && true) => true
                                    return ValueExpression.Create(0);
                                }
                            }
                            else
                            {
                                // (true && ??) => ??
                                return _last;
                            }
                        }
                        else if (last is IConstExpression n2)
                        {
                            if (n2.Value == 0)
                            {
                                // (?? && false) => false
                                return ValueExpression.Create(1);
                            }

                            // (?? && true) => ??
                            return first;
                        }

                        // (?? && ??)
                        return new LogAnd(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" && ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new LogOr(_first.LogicalInverse(), _last.LogicalInverse());

                    public override string ToString() => $"Eval({_first} && {_last})";

                    #endregion Methods
                }

                public class LogNot : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _value;

                    #endregion Fields

                    #region Constructors

                    public LogNot(IJsmExpression value) => _value = value;

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _value.Calculate(services) == 0 ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var result = _value.Evaluate(services);
                        if (result is IConstExpression number)
                            return ValueExpression.Create(number.Value == 0 ? 1 : 0);
                        return result;
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("!");
                        _value.Format(sw, formatterContext, services);
                    }

                    public ILogicalExpression LogicalInverse() => throw new NotImplementedException();//return _value;

                    public override string ToString() => $"Eval(!{_value})";

                    #endregion Methods
                }

                public class LogOr : ILogicalExpression
                {
                    #region Fields

                    private readonly ILogicalExpression _first;
                    private readonly ILogicalExpression _last;

                    #endregion Fields

                    #region Constructors

                    public LogOr(ILogicalExpression first, ILogicalExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => (_first.Calculate(services) != 0 || _last.Calculate(services) != 0) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.LogicalEvaluate(services);
                        var last = _last.LogicalEvaluate(services);

                        if (first is IConstExpression n1)
                        {
                            if (n1.Value != 0)
                            {
                                // (true || ??) => true
                                return ValueExpression.Create(0);
                            }

                            if (last is IConstExpression n2)
                            {
                                if (n2.Value != 0)
                                {
                                    // (false || true) => true
                                    return ValueExpression.Create(0);
                                }
                                else
                                {
                                    // (false || false) => false
                                    return ValueExpression.Create(1);
                                }
                            }
                            else
                            {
                                // (false || ??) => ??
                                return _last;
                            }
                        }
                        else if (last is IConstExpression n2)
                        {
                            if (n2.Value != 0)
                            {
                                // (?? || true) => true
                                return ValueExpression.Create(0);
                            }

                            // (?? || true) => ??
                            return first;
                        }

                        // (?? || ??)
                        return new LogOr(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" || ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new LogAnd(_first.LogicalInverse(), _last.LogicalInverse());

                    public override string ToString() => $"Eval({_first} || {_last})";

                    #endregion Methods
                }

                public class Minus : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _value;

                    #endregion Fields

                    #region Constructors

                    public Minus(IJsmExpression value) => _value = value;

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => -(_value.Calculate(services));

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var result = _value.Evaluate(services);
                        if (result is IConstExpression number)
                            return ValueExpression.Create((int)(-number.Value));
                        return result;
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("-");
                        _value.Format(sw, formatterContext, services);
                    }

                    public override string ToString() => $"Eval(-{_value})";

                    #endregion Methods
                }

                public class Mod : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Mod(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) % _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value % n2.Value));
                        return new Mod(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" % ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} % {_last})";

                    #endregion Methods
                }

                public class Mul : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Mul(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) * _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value * n2.Value));
                        return new Mul(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" * ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} * {_last})";

                    #endregion Methods
                }

                public class NotEquals : ILogicalExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public NotEquals(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) != _last.Calculate(services) ? 1 : 0;

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create(n1.Value != n2.Value ? 1 : 0);
                        return new NotEquals(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" != ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public ILogicalExpression LogicalInverse() => new Eq(_first, _last);

                    public override string ToString() => $"Eval({_first} != {_last})";

                    #endregion Methods
                }

                public class Sub : IJsmExpression
                {
                    #region Fields

                    private readonly IJsmExpression _first;
                    private readonly IJsmExpression _last;

                    #endregion Fields

                    #region Constructors

                    public Sub(IJsmExpression first, IJsmExpression last)
                    {
                        _first = first;
                        _last = last;
                    }

                    #endregion Constructors

                    #region Methods

                    public long Calculate(IServices services) => _first.Calculate(services) - _last.Calculate(services);

                    public IJsmExpression Evaluate(IServices services)
                    {
                        var first = _first.Evaluate(services);
                        var last = _last.Evaluate(services);
                        if (first is IConstExpression n1 && last is IConstExpression n2)
                            return ValueExpression.Create((int)(n1.Value - n2.Value));
                        return new Sub(first, last);
                    }

                    public void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
                    {
                        sw.Append("(");
                        _first.Format(sw, formatterContext, services);
                        sw.Append(" - ");
                        _last.Format(sw, formatterContext, services);
                        sw.Append(")");
                    }

                    public override string ToString() => $"Eval({_first} - {_last})";

                    #endregion Methods
                }

                #endregion Classes
            }

            #endregion Classes
        }

        #endregion Classes
    }
}