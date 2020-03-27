using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public sealed class FieldObjectScripts
    {
        #region Fields

        private readonly OrderedDictionary<int, IScriptExecutor> _dic = new OrderedDictionary<int, IScriptExecutor>();
        private readonly PriorityQueue<Executer> _executionQueue = new PriorityQueue<Executer>();

        private Executer _currentExecutor;
        private IAwaiter _currentState;
        private bool _isInitialized;

        #endregion Fields

        #region Methods

        public void Add(int scriptId, IScriptExecutor executor) => _dic.Add(scriptId, executor);

        public void CancelAll()
        {
            while (_executionQueue.TryDequeue(out var executor))
                executor.Complete();
        }

        /// <summary>
        /// Requests that a remote entity executes one of its member functions at a specified priority.
        /// The request is asynchronous and returns immediately without waiting for the remote execution to start or finish.
        /// If the specified priority is already busy executing, the request will block until it becomes available and only then return
        /// </summary>
        public IAwaitable Execute(int scriptId, int priority)
        {
            var executer = new Executer(GetScriptExecuter(scriptId));
            _executionQueue.Enqueue(executer, priority);
            return executer;
        }

        /// <summary>
        /// Requests that a remote entity executes one of its member functions at a specified priority.
        /// The request is asynchronous and returns immediately without waiting for the remote execution to start or finish.
        /// If the specified priority is already busy executing, the request will fail silently.
        /// </summary>
        public IAwaitable TryExecute(int scriptId, int priority)
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

                if (_currentExecutor != null)
                {
                    while (_currentExecutor.Next(out var current))
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

                    _currentExecutor.Complete();
                    _currentExecutor = null;
                }

                if (_executionQueue.Count <= 0)
                    break;

                _currentExecutor = _executionQueue.Dequeue();
                _currentExecutor.Execute(services);
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

        private IScriptExecutor GetScriptExecuter(int scriptId)
        {
            if (_dic.TryGetByKey(scriptId, out var obj))
                return obj;

            throw new ArgumentException($"A script (Id: {scriptId}) isn't exists.", nameof(scriptId));
        }

        #endregion Methods

        #region Classes

        private sealed class Executer : IAwaitable, IAwaiter
        {
            #region Fields

            private readonly IScriptExecutor _executor;
            private IEnumerator<IAwaitable> _en;

            #endregion Fields

            #region Constructors

            public Executer(IScriptExecutor executor) => _executor = executor;

            #endregion Constructors

            #region Properties

            public bool IsCompleted { get; set; }

            #endregion Properties

            #region Methods

            public void Complete() => IsCompleted = true;

            public void Execute(IServices services)
            {
                if (IsCompleted)
                    throw new InvalidOperationException("The script has already been executed once.");

                _en = _executor.Execute(services).GetEnumerator();
            }

            public IAwaiter GetAwaiter() => this;

            public void GetResult()
            {
            }

            public bool Next(out IAwaitable awaitable)
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

            public void OnCompleted(Action continuation) => continuation();

            #endregion Methods
        }

        #endregion Classes
    }
}