namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Response model for listing tenants.
/// </summary>
public class ListTenantsResponse
{
    /// <summary>
    /// List of tenants.
    /// </summary>
    public List<TenantModel> Tenants { get; set; } = new();
}
