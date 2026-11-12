
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesFlow.Application.Abstractions;
using SalesFlow.Application.Features.Activities;
using SalesFlow.Application.Features.Customers;
using SalesFlow.Application.Features.Deals;
using SalesFlow.Application.Features.Enterprise;
using SalesFlow.Application.Features.Leads;
using SalesFlow.Application.Features.Members;
using SalesFlow.Application.Features.Products;
using SalesFlow.Application.Features.Tasks;
using SalesFlow.Application.Features.Tenants;
using SalesFlow.Application.Security;
using SalesFlow.Infrastructure.Identity;
using SalesFlow.Infrastructure.Persistence;
using SalesFlow.Infrastructure.Persistence.Repositories;
using SalesFlow.Infrastructure.Persistence.Reports;

namespace SalesFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SalesFlow");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:SalesFlow is missing. Use user-secrets or environment variables.");

        // HTTP & Current User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        // PostgreSQL & Unit of Work
        services.AddDbContext<SalesFlowDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SalesFlowDbContext>());

        // Repositories
        services.AddScoped(typeof(ITenantRepository<>), typeof(EfTenantRepository<>));
        services.AddScoped<ITenantMemberRepository, EfTenantMemberRepository>();
        services.AddScoped<ITenantCatalog, EfTenantCatalog>();
        services.AddScoped<IIdentityDirectory, IdentityDirectory>();

        // Security
        services.AddScoped<AccessGuard>();

        // Tenant Services
        services.AddScoped<TenantOnboardingService>();
        services.AddScoped<TenantService>();
        services.AddScoped<MemberService>();

        // Customer Services
        services.AddScoped<CustomerService>();
        services.AddScoped<ContactService>();

        // Sales Services
        services.AddScoped<LeadService>();
        services.AddScoped<DealService>();
        services.AddScoped<TaskService>();
        services.AddScoped<ActivityService>();

        // Product Services
        services.AddScoped<IProductService, ProductService>();

        // Enterprise Product & Deal Products
        services.AddScoped<EnterpriseProductService>();
        services.AddScoped<EnterpriseDealProductService>();

        // Quotes
        services.AddScoped<EnterpriseQuoteService>();

        // Orders, Invoices & Payments
        services.AddScoped<EnterpriseBillingService>();

        // Pipelines
        services.AddScoped<EnterprisePipelineService>();

        // CRM Record Security
        services.AddScoped<EnterpriseRecordGuard>();

        // Notes & Tags
        services.AddScoped<EnterpriseCollaborationService>();

        // Custom Fields
        services.AddScoped<EnterpriseCustomFieldService>();

        // Sales Goals
        services.AddScoped<EnterpriseGoalService>();

        // Audit
        services.AddScoped<EnterpriseAuditService>();

        // Dashboard & Reports
        services.AddScoped<EnterpriseDashboardService>();
        services.AddScoped<ISalesDashboardReader, EfSalesDashboardReader>();

        // ASP.NET Core Identity
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
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<SalesFlowDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
