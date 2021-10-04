using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachines
{
    public abstract class BaseState<TContext> where TContext : IContext
    {
        public virtual string DebugData { get; }
        public event Action<string> DebugMesage;

        protected string _parentStateMachineName { get; private set; }
        private float _startTime;
        private List<(float, Action)> _delays = new List<(float, Action)>();

        public BaseState<TContext> Init(TContext context, string parentStateMachineName)
        {
            _parentStateMachineName = parentStateMachineName;
            OnInit(context);
            return this;
        }

        protected virtual void OnInit(TContext context) { }

        public void Enter(TContext context)
        {
            _startTime = Time.time;
            OnEnter(context);
        }

        protected virtual void OnEnter(TContext context) { }

        public void Exit(TContext context)
        {
            _delays.Clear();
            OnExit(context);
        }

        protected virtual void OnExit(TContext context) { }

        public void Update(TContext context)
        {
            var invoked = new List<int>();
            for (int i = 0; i < _delays.Count; i++)
            {
                var delay = _delays[i];
                if (delay.Item1 <= Time.time)
                {
                    delay.Item2?.Invoke();
                    if (_delays.Count == 0)
                        return;
                    invoked.Add(i);
                }
            }
            for (int i = invoked.Count - 1; i >= 0; i--)
                _delays.RemoveAt(invoked[i]);
            OnUpdate(context);
        }

        protected virtual void OnUpdate(TContext context) { }

        protected void SetDelay(float delay, Action delayAction)
        {
            if (delayAction != null && delay >= 0f)
                _delays.Add((Time.time + delay, delayAction));
        }

        protected void SendDebugMessage(string message)
        {
            DebugMesage?.Invoke(message);
        }
    }
}