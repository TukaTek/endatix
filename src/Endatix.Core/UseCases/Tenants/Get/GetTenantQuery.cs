using Ardalis.GuardClauses;
using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Get;

/// <summary>
/// Query for getting a tenant by ID.
/// </summary>
public record GetTenantQuery : IQuery<Result<Tenant>>
{
    public long TenantId { get; init; }

    public GetTenantQuery(long tenantId)
    {
        Guard.Against.NegativeOrZero(tenantId);
        TenantId = tenantId;
    }
}
