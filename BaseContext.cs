using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StateMachines
{
    public abstract class BaseContext<TContext> : MonoBehaviour, IContext where TContext : BaseContext<TContext>
    {
        private Dictionary<Type, IHaveContext<TContext>> _components;

        private void Awake()
        {
            _components = GetComponents<IHaveContext<TContext>>().Union(GetComponentsInChildren<IHaveContext<TContext>>())
                .Select(s => { s.SetContext((TContext)this); return s; })
                .ToDictionary(s => s.GetType());
            OnAwake();
        }

        protected virtual void OnAwake() { }

        public T Get<T>() where T : IHaveContext<TContext>
        {
            var type = typeof(T);
            foreach (var entry in _components)
                if (type.IsAssignableFrom(entry.Key))
                    return (T)entry.Value;
            return default(T);
        }

        public IEnumerable<T> GetAll<T>() where T : IHaveContext<TContext>
        {
            var type = typeof(T);
            foreach (var entry in _components)
                if (type.IsAssignableFrom(entry.Key))
                    yield return (T)entry.Value;
        }
    }
}
