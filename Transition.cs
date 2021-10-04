using System;
using UnityEngine;

namespace StateMachines
{
    public partial class StateMachine<TContext> where TContext : IContext
    {
        private class Transition<Tcontext> where Tcontext : IContext
        {
            public readonly Type TargetState;
            public readonly Func<TContext, bool> Condition;
            public readonly Func<TContext, float> DurationFunc;
            public float Duration { get; private set; }
            public float StartTime { get; private set; }
            public Trigger Trigger { get; private set; }

            private Transition(Func<TContext, bool> condition, Type targetState)
            {
                TargetState = targetState;
                Condition = condition;
            }

            private Transition(Func<TContext, float> durationFunc, Type targetState)
            {
                TargetState = targetState;
                DurationFunc = durationFunc;
                Condition = c => Duration < (Time.time - StartTime);
            }

            private Transition(float duration, Type targetState)
            {
                TargetState = targetState;
                Duration = duration;
                Condition = c => Duration < (Time.time - StartTime);
            }

            private Transition(Trigger trigger, Type targetState)
            {
                TargetState = targetState;
                Trigger = trigger;
                Condition = c => Trigger.State;
            }

            public void ParentStateStarted(TContext context)
            {
                StartTime = Time.time;
                if (DurationFunc != null)
                    Duration = DurationFunc(context);
            }

            public void UnsetTrigger()
            {
                Trigger?.Unset();
            }

            public static Transition<Tcontext> New<TTargetState>(Func<TContext, bool> condition)
            {
                return new Transition<Tcontext>(condition, typeof(TTargetState));
            }

            public static Transition<Tcontext> New<TTargetState>(Func<TContext, float> durationFunc)
            {
                return new Transition<Tcontext>(durationFunc, typeof(TTargetState));
            }

            public static Transition<Tcontext> New<TTargetState>(float duration)
            {
                return new Transition<Tcontext>(duration, typeof(TTargetState));
            }

            public static Transition<Tcontext> New<TTargetState>(Trigger trigger)
            {
                return new Transition<Tcontext>(trigger, typeof(TTargetState));
            }
        }
    }
}