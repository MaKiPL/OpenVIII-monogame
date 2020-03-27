using System.Collections.Generic;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public sealed class LabeledStack : IStack<IJsmExpression>
        {
            #region Fields

            private readonly Dictionary<IJsmExpression, int> _positions = new Dictionary<IJsmExpression, int>();
            private readonly Stack<IJsmExpression> _stack = new Stack<IJsmExpression>();

            #endregion Fields

            #region Properties

            public int Count => _stack.Count;
            public int CurrentLabel { get; set; }

            #endregion Properties

            #region Methods

            public IJsmExpression Peek() => _stack.Peek();

            public IJsmExpression Pop()
            {
                var result = _stack.Pop();

                CurrentLabel = _positions[result];
                _positions.Remove(result);

                return result;
            }

            public void Push(IJsmExpression item)
            {
                _positions.Add(item, CurrentLabel);
                _stack.Push(item);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}