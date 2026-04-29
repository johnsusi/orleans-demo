using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RockSolid.Foundation.Modeling;

namespace Domain;

public sealed record DeviceId(Guid Value) : IValueObject<DeviceId>;
public sealed record DevicePropertyName(string Value) : IValueObject<DevicePropertyName>;
public sealed partial class DevicePropertyValue : IValueObject<DevicePropertyValue>;
public sealed record DevicePropertyAdded(DeviceId DeviceId, DevicePropertyName Name, DevicePropertyValue Value) : IDomainEvent<DevicePropertyAdded>;
public sealed record DevicePropertyModified(DeviceId DeviceId, DevicePropertyName Name, DevicePropertyValue OldValue, DevicePropertyValue NewValue) : IDomainEvent<DevicePropertyModified>;
public sealed record DevicePropertyRemoved(DeviceId DeviceId, DevicePropertyName Name, DevicePropertyValue OldValue) : IDomainEvent<DevicePropertyRemoved>;

public sealed class Device(DeviceId deviceId) : AggregateRoot<Device, DeviceId>(deviceId)
{

    private readonly Dictionary<DevicePropertyName, DevicePropertyValue> _properties = [];

    public void SetProperty(DevicePropertyName name, DevicePropertyValue? value)
    {
        if (value is null)
        {
            if (_properties.Remove(name, out var oldValue))
            {
                RaiseDomainEvent(new DevicePropertyRemoved(Id, name, oldValue));
            }
        }
        else if (_properties.TryGetValue(name, out var oldValue))
        {
            if (oldValue != value)
            {

                _properties[name] = value;
                RaiseDomainEvent(new DevicePropertyModified(Id, name, oldValue, value));
            }
        }
        else
        {
            _properties[name] = value;
            RaiseDomainEvent(new DevicePropertyAdded(Id, name, value));
        }
    }
}


public sealed class DeviceEntityTypeConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
    }
}