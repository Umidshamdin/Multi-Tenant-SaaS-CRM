using System.Text.RegularExpressions;
using SalesFlow.Domain.Common;
using SalesFlow.Domain.Exceptions;

namespace SalesFlow.Domain.Entities;
public sealed class Organization : TenantEntity
{
    public string LegalName { get; private set; } = string.Empty;
    public string? CountryCode { get; private set; }
    public string? WebsiteUrl { get; private set; }

    private Organization() { }

    public Organization(Guid tenantId, string legalName, string? countryCode = null, string? websiteUrl = null) : base(tenantId)
    {
        UpdateProfile(legalName, countryCode, websiteUrl);
    }
    public void UpdateProfile(string legalName, string? countryCode, string? websiteUrl)
    {
        var name = DomainRules.Required(legalName, nameof(LegalName), 200);
        var country = DomainRules.Optional(countryCode, nameof(CountryCode), 2)?.ToUpperInvariant();
        var website = DomainRules.Optional(websiteUrl, nameof(WebsiteUrl), 500);

        if (country is not null && !Regex.IsMatch(country, "^[A-Z]{2}$")) throw new DomainException("CountryCode must contain two letters.");

        if (website is not null &&
            (!Uri.TryCreate(website, UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
            throw new DomainException("WebsiteUrl must be an absolute HTTP or HTTPS URL.");

        LegalName = name;
        CountryCode = country;
        WebsiteUrl = website;
        Touch();
    }
}