
using Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

var builder = Host.CreateApplicationBuilder();
// builder.Services.AddOrleansClient(options =>
// {
//     options.UseLocalhostClustering();
// });

builder.Services.AddHostedService<Worker>();

using var app = builder.Build();

await app.RunAsync();
// await app.StartAsync();
// await using var scope = app.Services.CreateAsyncScope();
// var cluster = scope.ServiceProvider.GetRequiredService<IClusterClient>();
// var hello = cluster.GetGrain<IHelloGrain>(0);
// var response = await hello.greet("World");
// await app.StopAsync();



internal sealed class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithConnectionUri("ws://localhost:8080/mqtt")
                .Build())
            .Build();

        using var mqttClient = new MqttFactory().CreateManagedMqttClient();
        await mqttClient.SubscribeAsync([new MqttTopicFilterBuilder().WithTopic("my/topic").Build()]);
        await mqttClient.StartAsync(options);

        while (!cancellationToken.IsCancellationRequested)
        {
            await mqttClient.EnqueueAsync("World");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}