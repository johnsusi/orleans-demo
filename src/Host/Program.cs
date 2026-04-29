using Application;
using Domain;
using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using OpenTelemetry.Metrics;
using Orleans.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        .AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution")
        .AddMeter("Microsoft.Orleans")
        .AddPrometheusExporter());


builder.Services.AddOrleans(silo =>
{
    var orleans = builder.Configuration.GetSection("Orleans");
    if (orleans["UseClustering"] == "ado")
        silo.UseAdoNetClustering(clustering =>
        {
            clustering.Invariant = orleans["Invariant"];
            clustering.ConnectionString = orleans["ConnectionString"];
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

app.MapPrometheusScrapingEndpoint();
app.MapOrleansDashboard();

app.MapMqtt("/mqtt");

var grainFactory = app.Services.GetRequiredService<IGrainFactory>();

app.UseMqttServer(
    server =>
    {

        // server.ClientConnectedAsync += async args =>
        // {
        //     try
        //     {
        //         app.Logger.LogInformation("ClientConnectedAsync: {client}", args.ClientId);
        //         var grain = grainFactory.GetGrain<IHelloGrain>($"hello/{args.ClientId}");
        //         await grain.Connect();
        //     }
        //     catch (Exception err)
        //     {
        //         app.Logger.LogError(err, "Connected");
        //     }
        // };

        server.InterceptingPublishAsync += async args =>
        {
            try
            {
                app.Logger.LogInformation("InterceptingPublishAsync: {client}: {topic}", args.ClientId, args.ApplicationMessage.Topic);

                var grain = grainFactory.GetGrain<IDeviceGrain>(Guid.Parse(args.ClientId));
                if (args.ApplicationMessage.Topic == $"devices/{args.ClientId}/telemetry")
                {
                    try
                    {
                        var message = DeviceTelemetry.Parser.ParseFrom(args.ApplicationMessage.Payload);
                        await grain.ReportTelemetryAsync(message, args.CancellationToken);
                    }
                    catch (Exception err)
                    {
                        app.Logger.LogError(err, "Unable to report telemetry");
                        args.Response.ReasonCode = MqttPubAckReasonCode.PayloadFormatInvalid;
                    }
                }
                else
                {
                    args.Response.ReasonCode = MqttPubAckReasonCode.NotAuthorized;
                }
            }
            catch (Exception err)
            {
                app.Logger.LogError(err, "Publish");
            }

        };

        // server.ClientDisconnectedAsync += async args =>
        // {
        //     try
        //     {
        //         app.Logger.LogInformation("ClientDisconnectedAsync: {client}", args.ClientId);
        //         var grain = grainFactory.GetGrain<IHelloGrain>($"hello/{args.ClientId}");
        //         await grain.Disconnect();
        //     }
        //     catch (Exception err)
        //     {
        //         app.Logger.LogError(err, "Disconnected");
        //     }

        // };

    });

app.Run();

