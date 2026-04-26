namespace Application;

public interface IHelloGrain : IGrainWithIntegerKey
{

    public Task<string> greet(string who, CancellationToken cancellationToken = default);

}

public class HelloGrain : Grain, IHelloGrain
{

    public Task<string> greet(string who, CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<string>(cancellationToken)
            : Task.FromResult($"Hello, {who}!");
    }

}
