using System;
using System.Collections.Generic;

namespace FF8
{
    public sealed class PriorityQueue<T>
    {
        private LinkedList<Item> _list;

        public Int32 Count => _list.Count;

        public PriorityQueue()
        {
            _list = new LinkedList<Item>();
        }

        public void Enqueue(T value, Int32 priority = 0)
        {
            Item item = new Item(value, priority);

            if (_list.Count == 0)
            {
                _list.AddLast(item);
                return;
            }

            Int32 first = _list.First.Value.Priority;
            Int32 last = _list.Last.Value.Priority;

            if (Math.Abs(first - priority) < Math.Abs(last - priority))
            {
                for (var i = _list.First; i != null; i = i.Next)
                {
                    if (i.Value.Priority > priority)
                    {
                        _list.AddBefore(i, item);
                        return;
                    }
                }
            }
            else
            {
                for (var i = _list.Last; i != null; i = i.Previous)
                {
                    if (i.Value.Priority <= priority)
                    {
                        _list.AddAfter(i, item);
                        return;
                    }
                }
            }

            _list.AddLast(item);
        }

        public T Dequeue()
        {
            if (TryDequeue(out var value))
                return value;

            throw new InvalidOperationException("The queue is empty.");
        }

        public Boolean TryDequeue(out T value)
        {
            if (Count == 0)
            {
                value = default(T);
                return false;
            }

            var item = _list.First.Value;
            _list.RemoveFirst();

            value = item.Value;
            return true;
        }

        public Boolean HasPriority(Int32 priority)
        {
            if (_list.Count == 0)
                return false;

            Int32 first = _list.First.Value.Priority;
            Int32 last = _list.Last.Value.Priority;

            if (Math.Abs(first - priority) < Math.Abs(last - priority))
            {
                for (var i = _list.First; i != null; i = i.Next)
                {
                    if (i.Value.Priority == priority)
                        return true;

                    if (i.Value.Priority > priority)
                        return false;
                }
            }
            else
            {
                for (var i = _list.Last; i != null; i = i.Previous)
                {
                    if (i.Value.Priority == priority)
                        return true;

                    if (i.Value.Priority < priority)
                        return false;
                }
            }

            return false;
        }

        public void Clear()
        {
            _list.Clear();
        }

        private sealed class Item
        {
            public readonly T Value;
            public readonly Int32 Priority;

            public Item(T value, Int32 priority)
            {
                Value = value;
                Priority = priority;
            }
        }
    }
}