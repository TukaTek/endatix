using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Domain;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Create;

/// <summary>
/// Handler for creating a new tenant.
/// </summary>
public class CreateTenantHandler(IRepository<Tenant> tenantRepository)
    : ICommandHandler<CreateTenantCommand, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant(request.Name, request.Description);

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await tenantRepository.SaveChangesAsync(cancellationToken);

        return Result<Tenant>.Created(tenant);
    }
}
