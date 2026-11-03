using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SalesFlow.Infrastructure.Persistence;

// Domain currently creates local DateTime values. Keep wall-clock values until the UTC migration.
internal sealed class LocalDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public LocalDateTimeConverter() : base(value => DateTime.SpecifyKind(value, DateTimeKind.Unspecified), value => DateTime.SpecifyKind(value, DateTimeKind.Local)) { }
}
