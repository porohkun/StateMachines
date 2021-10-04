using System;

namespace StateMachines
{
    public partial class StateMachine<TContext> where TContext : IContext
    {
        public class AddStateContext<TStateFrom> where TStateFrom : BaseState<TContext>
        {
            private StateMachine<TContext> _stateMachine;

            internal AddStateContext(StateMachine<TContext> stateMachine)
            {
                _stateMachine = stateMachine;
            }

            public AddStateContext<TStateFrom> AddTransition<TStateTo>(Func<TContext, bool> condition)
                where TStateTo : BaseState<TContext>
            {
                _stateMachine.AddTransition<TStateFrom, TStateTo>(condition);
                return this;
            }

            public AddStateContext<TStateFrom> AddTransition<TStateTo>(Func<TContext, float> durationFunc)
                where TStateTo : BaseState<TContext>
            {
                _stateMachine.AddTransition<TStateFrom, TStateTo>(durationFunc);
                return this;
            }

            public AddStateContext<TStateFrom> AddTransition<TStateTo>(float duration = 0f)
                where TStateTo : BaseState<TContext>
            {
                _stateMachine.AddTransition<TStateFrom, TStateTo>(duration);
                return this;
            }

            public AddStateContext<TStateFrom> AddTransition<TStateTo>(Trigger trigger)
                where TStateTo : BaseState<TContext>
            {
                _stateMachine.AddTransition<TStateFrom, TStateTo>(trigger);
                return this;
            }
        }
    }
}