namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Request model for getting a tenant by ID.
/// </summary>
public class GetTenantRequest
{
    /// <summary>
    /// The ID of the tenant.
    /// </summary>
    public long Id { get; set; }
}
