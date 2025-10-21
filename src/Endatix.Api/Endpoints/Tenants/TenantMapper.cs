using Endatix.Core.Entities;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Mapper for Tenant entity to API models.
/// </summary>
public static class TenantMapper
{
    /// <summary>
    /// Maps a Tenant entity to a TenantModel.
    /// </summary>
    public static TModel Map<TModel>(Tenant tenant) where TModel : TenantModel, new()
    {
        return new TModel
        {
            Id = tenant.Id.ToString(),
            Name = tenant.Name,
            Description = tenant.Description,
            CreatedAt = tenant.CreatedAt,
            ModifiedAt = tenant.ModifiedAt
        };
    }
}
