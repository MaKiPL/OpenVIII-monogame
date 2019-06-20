using System;
using System.Collections.Generic;


namespace FF8
{
    public sealed class FieldObjectScripts
    {
        private readonly OrderedDictionary<Int32, IScriptExecuter> _dic = new OrderedDictionary<Int32, IScriptExecuter>();
        private readonly PriorityQueue<Executer> _executionQueue = new PriorityQueue<Executer>();

        private Boolean _isInitialized;
        private IAwaiter _currentState;
        private Executer _currentExecuter;

        public void Add(Int32 scriptId, IScriptExecuter executer)
        {
            _dic.Add(scriptId, executer);
        }

        public void CancelAll()
        {
            while (_executionQueue.TryDequeue(out var executer))
                executer.Complete();
        }

        /// <summary>
        /// Requests that a remote entity executes one of its member functions at a specified priority.
        /// The request is asynchronous and returns immediately without waiting for the remote execution to start or finish.
        /// If the specified priority is already busy executing, the request will block until it becomes available and only then return
        /// </summary>
        public IAwaitable Execute(Int32 scriptId, Int32 priority)
        {
            Executer executer = new Executer(GetScriptExecuter(scriptId));
            _executionQueue.Enqueue(executer, priority);
            return executer;
        }

        /// <summary>
        /// Requests that a remote entity executes one of its member functions at a specified priority.
        /// The request is asynchronous and returns immediately without waiting for the remote execution to start or finish.
        /// If the specified priority is already busy executing, the request will fail silently. 
        /// </summary>
        public IAwaitable TryExecute(Int32 scriptId, Int32 priority)
        {
            if (_executionQueue.HasPriority(priority))
                return DummyAwaitable.Instance;

            return Execute(scriptId, priority);
        }

        public void Update(IServices services)
        {
            EnsureInitialized();

            while (true)
            {
                if (_currentState != null)
                {
                    if (!_currentState.IsCompleted)
                        return;

                    _currentState = null;
                }

                if (_currentExecuter != null)
                {
                    while (_currentExecuter.Next(out IAwaitable current))
                    {
                        if (current == null)
                            throw new InvalidOperationException();

                        if (current == BreakAwaitable.Instance)
                            break;

                        if (current == SpinAwaitable.Instance)
                            return;

                        _currentState = current.GetAwaiter();
                        if (!_currentState.IsCompleted)
                            return;
                    }

                    _currentState = null;

                    _currentExecuter.Complete();
                    _currentExecuter = null;
                }

                if (_executionQueue.Count <= 0)
                    break;

                _currentExecuter = _executionQueue.Dequeue();
                _currentExecuter.Execute(services);
            }
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            foreach (var item in _dic.Values)
                _executionQueue.Enqueue(new Executer(item));

            _isInitialized = true;
        }

        private IScriptExecuter GetScriptExecuter(Int32 scriptId)
        {
            if (_dic.TryGetByKey(scriptId, out var obj))
                return obj;

            throw new ArgumentException($"A script (Id: {scriptId}) isn't exists.", nameof(scriptId));
        }

        private sealed class Executer : IAwaitable, IAwaiter
        {
            private readonly IScriptExecuter _executer;
            private IEnumerator<IAwaitable> _en;

            public Executer(IScriptExecuter executer)
            {
                _executer = executer;
            }

            public Boolean IsCompleted { get; private set; }
            public void Complete() => IsCompleted = true;

            public void Execute(IServices services)
            {
                if (IsCompleted)
                    throw new InvalidOperationException("The script has already been executed once.");

                _en = _executer.Execute(services).GetEnumerator();
            }

            public Boolean Next(out IAwaitable awaitable)
            {
                if (_en == null)
                    throw new InvalidOperationException("The script hasn't yet started.");

                if (_en.MoveNext())
                {
                    awaitable = _en.Current;
                    return true;
                }

                IsCompleted = true;
                awaitable = null;
                return false;
            }

            public IAwaiter GetAwaiter()
            {
                return this;
            }

            public void OnCompleted(Action continuation)
            {
                continuation();
            }

            public void GetResult()
            {
            }
        }
    }
}