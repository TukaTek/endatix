using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Domain;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.List;

/// <summary>
/// Handler for listing all tenants.
/// </summary>
public class ListTenantsHandler(IRepository<Tenant> tenantRepository)
    : IQueryHandler<ListTenantsQuery, Result<IEnumerable<Tenant>>>
{
    public async Task<Result<IEnumerable<Tenant>>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await tenantRepository.ListAsync(cancellationToken);
        return Result<IEnumerable<Tenant>>.Success(tenants);
    }
}
