using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Features.Activities;
using SalesFlow.Application.Features.Customers;
using SalesFlow.Application.Features.Deals;
using SalesFlow.Application.Features.Leads;
using SalesFlow.Application.Features.Members;
using SalesFlow.Application.Features.Products;
using SalesFlow.Application.Features.Tasks;
using SalesFlow.Application.Features.Tenants;
using SalesFlow.Application.Security;
using SalesFlow.Infrastructure.Identity;
using SalesFlow.Infrastructure.Persistence;
using SalesFlow.Infrastructure.Persistence.Repositories;

namespace SalesFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SalesFlow");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:SalesFlow is missing. Use user-secrets or environment variables.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddDbContext<SalesFlowDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SalesFlowDbContext>());
        services.AddScoped(typeof(ITenantRepository<>), typeof(EfTenantRepository<>));
        services.AddScoped<ITenantMemberRepository, EfTenantMemberRepository>();
        services.AddScoped<ITenantCatalog, EfTenantCatalog>();
        services.AddScoped<IIdentityDirectory, IdentityDirectory>();
        services.AddScoped<AccessGuard>();

        services.AddScoped<TenantOnboardingService>();
        services.AddScoped<TenantService>();
        services.AddScoped<MemberService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ContactService>();
        services.AddScoped<LeadService>();
        services.AddScoped<DealService>();
        services.AddScoped<TaskService>();
        services.AddScoped<ActivityService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddIdentityCore<SalesFlowUser>(options =>
        {
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<SalesFlowDbContext>().AddDefaultTokenProviders();

        return services;
    }
}