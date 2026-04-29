using Domain;

namespace Application;

public interface IDeviceGrain : IGrainWithGuidKey
{
    public Task ReportTelemetryAsync(DeviceTelemetry message, CancellationToken cancellationToken);

}

public class DeviceGrain : Grain, IDeviceGrain
{
    private readonly ApplicationDbContext _db;
    private Device? _device;

    public DeviceGrain(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task ReportTelemetryAsync(DeviceTelemetry message, CancellationToken cancellationToken)
    {
        _device ??= await _db.Devices.FindAsync([this.GetPrimaryKey()]);

        if (_device is null)
        {
            _device = new Device(new DeviceId(this.GetPrimaryKey()));
            _db.Add(_device);
        }

        var name = new DevicePropertyName(message.Name);
        foreach (var value in message.Values)

            _device.SetProperty(name, value);

        await _db.SaveChangesAsync(cancellationToken);
    }

}
