
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

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
        var options = new MqttClientOptionsBuilder()
            .WithClientId("foo")
            .WithConnectionUri("ws://localhost:8081/mqtt")
            .Build();

        using var mqttClient = new MqttFactory().CreateMqttClient();
        await mqttClient.ConnectAsync(options);
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Publishing Message");
            await mqttClient.PublishStringAsync(
                topic: "hello",
                payload: "Demo",
                MqttQualityOfServiceLevel.AtLeastOnce,
                retain: false,
                cancellationToken);
            Console.WriteLine("Message Published");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}