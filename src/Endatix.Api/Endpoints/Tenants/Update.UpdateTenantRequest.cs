namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Request model for updating a tenant.
/// </summary>
public class UpdateTenantRequest
{
    /// <summary>
    /// The ID of the tenant to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The new name of the tenant.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The new description of the tenant.
    /// </summary>
    public string? Description { get; set; }
}
