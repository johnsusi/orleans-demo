using Application;
using MQTTnet.AspNetCore;
using Orleans.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOrleans(silo =>
{
    var orleans = builder.Configuration.GetSection("Orleans");
    if (orleans["UseClustering"] == "ado")
        silo.UseAdoNetClustering(clustering =>
        {
            clustering.Invariant = orleans["Invariant"];
            clustering.ConnectionString = orleans["ConnectionString"];
            // clustering.Invariant = "Npgsql";
            // clustering.ConnectionString = "Host=postgres;Database=demo;Username=postgres;Password=secret";
        });
    else
        silo.UseLocalhostClustering();
    silo.AddAdoNetGrainStorage("helloStore", storage =>
    {
        storage.Invariant = orleans["Invariant"];
        storage.ConnectionString = orleans["ConnectionString"];
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

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

app.UseMqttServer(
    server =>
    {

        server.ClientConnectedAsync += async args =>
        {
            try
            {
                app.Logger.LogInformation("ClientConnectedAsync: {client}", args.ClientId);
                var grain = grainFactory.GetGrain<IHelloGrain>($"hello/{args.ClientId}");
                await grain.Connect();
            }
            catch (Exception err)
            {
                app.Logger.LogError(err, "Connected");
            }
        };

        server.InterceptingPublishAsync += async args =>
        {
            try
            {
                app.Logger.LogInformation("InterceptingPublishAsync: {client}: {topic}", args.ClientId, args.ApplicationMessage.Topic);
                var grain = grainFactory.GetGrain<IHelloGrain>($"hello/{args.ApplicationMessage.Topic}");
                await grain.Message(args.ClientId, args.CancellationToken);
            }
            catch (Exception err)
            {
                app.Logger.LogError(err, "Publish");
            }

        };

        server.ClientDisconnectedAsync += async args =>
        {
            try
            {
                app.Logger.LogInformation("ClientDisconnectedAsync: {client}", args.ClientId);
                var grain = grainFactory.GetGrain<IHelloGrain>($"hello/{args.ClientId}");
                await grain.Disconnect();
            }
            catch (Exception err)
            {
                app.Logger.LogError(err, "Disconnected");
            }

        };

    });

app.Run();

