using Ardalis.GuardClauses;
using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Delete;

/// <summary>
/// Command for deleting a tenant (soft delete).
/// </summary>
public record DeleteTenantCommand : ICommand<Result<Tenant>>
{
    public long TenantId { get; init; }

    public DeleteTenantCommand(long tenantId)
    {
        Guard.Against.NegativeOrZero(tenantId);
        TenantId = tenantId;
    }
}
