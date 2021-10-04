using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMachines
{
    public partial class StateMachine<TContext> where TContext : IContext
    {
        public string ID { get; private set; }
        public bool Logging { get; set; }
        public bool Active { get; private set; }
        public Type CurrentState => _currentState.Item1.GetType();
        public string DebugData => $"{_currentState.Item1.GetType().Name}\n{_currentState.Item1.DebugData}";

        public event Action<string> DebugMesage;

        private readonly TContext _context;
        private Type _startState;
        private Tuple<BaseState<TContext>, List<Transition<TContext>>> _currentState;
        private readonly Dictionary<Type, Tuple<BaseState<TContext>, List<Transition<TContext>>>> _states = new Dictionary<Type, Tuple<BaseState<TContext>, List<Transition<TContext>>>>();
        private readonly List<Transition<TContext>> _globalTransitions = new List<Transition<TContext>>();

        public StateMachine(string id, TContext context)
        {
            ID = id;
            _context = context;
        }

        public void Clear()
        {
            Active = false;
            _startState = null;
            _currentState = null;
            _states.Clear();
            _globalTransitions.Clear();
        }

        public AddStateContext<TState> AddState<TState>() where TState : BaseState<TContext>, new()
        {
            _states[typeof(TState)] = new Tuple<BaseState<TContext>, List<Transition<TContext>>>(new TState().Init(_context, ID), new List<Transition<TContext>>());
            if (Active && _currentState == null)
                ChangeCurrentState(_states.Keys.First());
            if (_startState == null)
                _startState = typeof(TState);
            return new AddStateContext<TState>(this);
        }

        public TState GetState<TState>() where TState : BaseState<TContext>
        {
            return (TState)_states[typeof(TState)].Item1;
        }
        public void ForceState<TState>() where TState : BaseState<TContext>
        {
            ForceState(typeof(TState));
        }

        private void ForceState(Type stateType)
        {
            ChangeCurrentState(stateType);
        }

        private void ChangeCurrentState(Type newState)
        {
            if (Logging)
                DebugMesage?.Invoke($"{ID} || '{_currentState?.Item1.GetType().Name}' --> '{newState.Name}'");
            if (_currentState != null)
            {
                _currentState.Item1.Exit(_context);
                _currentState.Item1.DebugMesage -= CurrentStateDebugMessageReceived;
            }
            _currentState = _states[newState];
            _currentState.Item1.DebugMesage += CurrentStateDebugMessageReceived;
            _currentState.Item1.Enter(_context);
            foreach (var transition in _currentState.Item2)
                transition.ParentStateStarted(_context);
        }

        private void CurrentStateDebugMessageReceived(string message)
        {
            if (Logging)
                DebugMesage?.Invoke(message);
        }

        #region transitions
        public void AddTransition<TStateFrom, TStateTo>(Func<TContext, bool> condition)
            where TStateFrom : BaseState<TContext>
            where TStateTo : BaseState<TContext>
        {
            _states[typeof(TStateFrom)].Item2.Add(Transition<TContext>.New<TStateTo>(condition));
        }

        public void AddTransition<TStateFrom, TStateTo>(Func<TContext, float> durationFunc)
            where TStateFrom : BaseState<TContext>
            where TStateTo : BaseState<TContext>
        {
            _states[typeof(TStateFrom)].Item2.Add(Transition<TContext>.New<TStateTo>(durationFunc));
        }

        public void AddTransition<TStateFrom, TStateTo>(float duration = 0f)
            where TStateFrom : BaseState<TContext>
            where TStateTo : BaseState<TContext>
        {
            _states[typeof(TStateFrom)].Item2.Add(Transition<TContext>.New<TStateTo>(duration));
        }

        public void AddTransition<TStateFrom, TStateTo>(Trigger trigger)
            where TStateFrom : BaseState<TContext>
            where TStateTo : BaseState<TContext>
        {
            _states[typeof(TStateFrom)].Item2.Add(Transition<TContext>.New<TStateTo>(trigger));
        }
        #endregion

        #region global transitions
        public void AddTransition<TStateTo>(Func<TContext, bool> condition)
            where TStateTo : BaseState<TContext>
        {
            _globalTransitions.Add(Transition<TContext>.New<TStateTo>(condition));
        }

        public void AddTransition<TStateTo>(Func<TContext, float> durationFunc)
            where TStateTo : BaseState<TContext>
        {
            _globalTransitions.Add(Transition<TContext>.New<TStateTo>(durationFunc));
        }

        public void AddTransition<TStateTo>(float duration = 0f)
            where TStateTo : BaseState<TContext>
        {
            _globalTransitions.Add(Transition<TContext>.New<TStateTo>(duration));
        }

        public void AddTransition<TStateTo>(Trigger trigger)
            where TStateTo : BaseState<TContext>
        {
            _globalTransitions.Add(Transition<TContext>.New<TStateTo>(trigger));
        }
        #endregion

        public void Start()
        {
            Active = true;
            ForceState(_startState);
        }

        public void Stop()
        {
            _currentState?.Item1.Exit(_context);
            Active = false;
        }

        public void Update()
        {
            if (!Active)
                return;
            bool stateChanged;
            int changesCount = 0;
            do
            {
                stateChanged = false;
                //optimization for foreach (var transition in _globalTransitions.Union(_currentState.Item2)
                for (int i = 0; i < _globalTransitions.Count + _currentState.Item2.Count; i++)
                {
                    var transition = i < _globalTransitions.Count ? _globalTransitions[i] : _currentState.Item2[i - _globalTransitions.Count];
                    //end of optimization
                    if (transition.TargetState != _currentState.Item1.GetType() && transition.Condition(_context))
                    {
                        transition.UnsetTrigger();
                        ChangeCurrentState(transition.TargetState);
                        stateChanged = true;
                        changesCount++;
                        break;
                    }
                }
            }
            while (stateChanged && changesCount < 16);
            _currentState.Item1.Update(_context);
        }
    }
}