using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Domain;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Get;

/// <summary>
/// Handler for getting a tenant by ID.
/// </summary>
public class GetTenantHandler(IRepository<Tenant> tenantRepository)
    : IQueryHandler<GetTenantQuery, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant == null)
        {
            return Result<Tenant>.NotFound();
        }

        return Result<Tenant>.Success(tenant);
    }
}
