namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Model of a tenant.
/// </summary>
public class TenantModel
{
    /// <summary>
    /// The ID of the tenant.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The name of the tenant.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The description of the tenant.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The date and time when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the tenant was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}
