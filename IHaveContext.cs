namespace StateMachines
{
    public interface IHaveContext<TContext> where TContext : IContext
    {
        void SetContext(TContext context);
    }
}
