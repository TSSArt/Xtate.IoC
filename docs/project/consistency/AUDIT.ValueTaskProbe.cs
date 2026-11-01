using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Xtate.IoC;

// Audit-only reproduction; production and test sources are unchanged.
internal static class Program
{
    private static async Task Main()
    {
        foreach (var actions in new[] { false, true })
        {
            var source = new SingleUseSource();
            await using var provider = Container.Create(services =>
            {
                if (actions) services.AddConstant<IServiceProviderActions>(new NoOpActions());
                services.AddForwarding<string>(_ => new ValueTask<string>(source, 0));
            });
            try
            {
                var value = await provider.GetRequiredService<string>();
                Console.WriteLine($"actions={actions}; result={value}; GetResult calls={source.Calls}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"actions={actions}; exception={ex.GetType().Name}: {ex.Message}; GetResult calls={source.Calls}");
            }
        }
    }

    private sealed class SingleUseSource : IValueTaskSource<string>
    {
        public int Calls;
        public string GetResult(short token) => ++Calls == 1 ? "resolved" : throw new InvalidOperationException("ValueTask consumed more than once");
        public ValueTaskSourceStatus GetStatus(short token) => ValueTaskSourceStatus.Succeeded;
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => throw new InvalidOperationException("Already complete");
    }

    private sealed class NoOpActions : IServiceProviderActions
    {
        public IServiceProviderDataActions? RegisterServices(int count) => null;
        public IServiceProviderDataActions? Event(ActionsEventType type, ref ActionsContext context) => null;
    }
}
