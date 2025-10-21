using Ardalis.GuardClauses;
using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Update;

/// <summary>
/// Command for updating a tenant.
/// </summary>
public record UpdateTenantCommand : ICommand<Result<Tenant>>
{
    public long TenantId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }

    public UpdateTenantCommand(long tenantId, string? name = null, string? description = null)
    {
        Guard.Against.NegativeOrZero(tenantId);

        TenantId = tenantId;
        Name = name;
        Description = description;
    }
}
