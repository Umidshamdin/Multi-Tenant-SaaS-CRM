using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SalesFlow.Infrastructure.Persistence.DesignTime;

public sealed class SalesFlowDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SalesFlowDbContext>
{
    public SalesFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SALESFLOW_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Set SALESFLOW_CONNECTION_STRING before running EF migrations.");
        var options = new DbContextOptionsBuilder<SalesFlowDbContext>().UseNpgsql(connectionString).Options;
        return new SalesFlowDbContext(options, new DesignTimeCurrentUser());
    }
}
