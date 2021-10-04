using UnityEngine;

namespace StateMachines
{
    public abstract class StateMachineConfiguration<TContext> : MonoBehaviour, IHaveContext<TContext> where TContext : IContext
    {
        [SerializeField] private bool _keepState;
        public bool KeepState => _keepState;

        protected TContext _context;

        public abstract void Configure(StateMachine<TContext> stateMachine);

        public void SetContext(TContext context)
        {
            _context = context;
        }
    }
}