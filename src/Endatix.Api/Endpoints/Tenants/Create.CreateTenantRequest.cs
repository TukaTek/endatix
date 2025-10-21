namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Request model for creating a tenant.
/// </summary>
public class CreateTenantRequest
{
    /// <summary>
    /// The name of the tenant.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The description of the tenant.
    /// </summary>
    public string? Description { get; set; }
}
