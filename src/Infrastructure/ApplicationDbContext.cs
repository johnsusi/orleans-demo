using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class DeviceEntityTypeConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id);
    }
}

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new DeviceEntityTypeConfiguration());
    }


}
