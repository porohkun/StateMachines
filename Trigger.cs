
using System;

namespace StateMachines
{
    public class Trigger
    {
        public bool State { get; private set; }
        public event Action<bool> StateChanged;

        public void Set()
        {
            State = true;
            StateChanged?.Invoke(State);
        }

        public void Unset()
        {
            State = false;
            StateChanged?.Invoke(State);
        }
    }
}