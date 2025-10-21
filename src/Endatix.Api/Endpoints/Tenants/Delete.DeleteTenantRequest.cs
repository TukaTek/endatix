namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Request model for deleting a tenant.
/// </summary>
public class DeleteTenantRequest
{
    /// <summary>
    /// The ID of the tenant to delete.
    /// </summary>
    public long Id { get; set; }
}
