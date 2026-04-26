using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using Orleans.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOrleans(silo =>
{
    silo.UseAdoNetClustering(clustering =>
    {
        clustering.Invariant = "Npgsql";
        clustering.ConnectionString = "Host=postgres;Database=demo;Username=postgres;Password=secret";

    });
    silo.Services.AddSingleton<PlacementStrategy, PreferLocalPlacement>();
    silo.AddDashboard();
});

builder.Services.AddHostedMqttServer(options =>
{
    options.WithDefaultEndpoint();
});

builder.Services.AddMqttConnectionHandler();
builder.Services.AddConnections();

var app = builder.Build();

app.UseRouting();

app.MapOrleansDashboard();

app.MapMqtt("/mqtt");

app.UseMqttServer(
    server =>
    {

        server.ApplicationMessageEnqueuedOrDroppedAsync += args =>
        {
            return Task.CompletedTask;
        };
        server.LoadingRetainedMessageAsync += args =>
        {
            app.Logger.LogInformation("LoadingRetainedMessageAsync");
            return Task.CompletedTask;
        };

        server.RetainedMessageChangedAsync += args =>
        {
            app.Logger.LogInformation("RetainedMessageChangedAsync");
            return Task.CompletedTask;
        };

        server.ValidatingConnectionAsync += args =>
        {
            app.Logger.LogInformation("ValidatingConnectionAsync");
            return Task.CompletedTask;
        };

        server.ClientConnectedAsync += args =>
        {
            app.Logger.LogInformation("ClientConnectedAsync");
            return Task.CompletedTask;
        };

        server.ClientDisconnectedAsync += args =>
        {
            app.Logger.LogInformation("ClientDisconnectedAsync");
            return Task.CompletedTask;
        };

        server.ClientSubscribedTopicAsync += args =>
        {
            app.Logger.LogInformation("ClientSubscribedTopicAsync");
            return Task.CompletedTask;
        };

        server.ClientUnsubscribedTopicAsync += args =>
        {
            app.Logger.LogInformation("ClientUnsubscribedTopicAsync");
            return Task.CompletedTask;
        };

        server.InterceptingClientEnqueueAsync += args =>
        {
            app.Logger.LogInformation("InterceptingClientEnqueueAsync");
            return Task.CompletedTask;
        };

        server.InterceptingPublishAsync += args =>
        {
            app.Logger.LogInformation("InterceptingPublishAsync: {client}: {topic}", args.ClientId, args.ApplicationMessage.Topic);
            return Task.CompletedTask;
        };

        server.InterceptingSubscriptionAsync += args =>
        {
            app.Logger.LogInformation("InterceptingSubscriptionAsync");
            args.Response.ReasonCode = MqttSubscribeReasonCode.NotAuthorized;

            return Task.CompletedTask;
        };

        server.InterceptingUnsubscriptionAsync += args =>
        {
            app.Logger.LogInformation("InterceptingUnsubscriptionAsync");
            return Task.CompletedTask;
        };


    });

app.Run();



