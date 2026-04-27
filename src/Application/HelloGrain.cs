using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;

namespace Application;

public interface IHelloGrain : IGrainWithStringKey
{
    public Task Connect(CancellationToken cancellationToken = default);
    public Task Message(string message, CancellationToken cancellationToken = default);
    public Task Disconnect(CancellationToken cancellationToken = default);
}

public class HelloGrain : Grain, IHelloGrain
{
    private readonly ILogger _logger;
    private readonly IPersistentState<HelloState> _state;

    public HelloGrain(
        ILogger<HelloGrain> logger,
        [PersistentState("hello", "helloStore")] IPersistentState<HelloState> state)
    {
        _logger = logger;
        _state = state;
    }

    public Task Connect(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Message(string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message: {} {}", message, _state.State.Value);
        _state.State.Value += 1;
        await _state.WriteStateAsync(cancellationToken);
    }

    public Task Disconnect(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

}


[GenerateSerializer]
public sealed record HelloState
{
    [Id(0)]
    public int Value { get; set; } = 0;

}