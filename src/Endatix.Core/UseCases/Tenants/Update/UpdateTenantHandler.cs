using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Domain;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Update;

/// <summary>
/// Handler for updating a tenant.
/// </summary>
public class UpdateTenantHandler(IRepository<Tenant> tenantRepository)
    : ICommandHandler<UpdateTenantCommand, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant == null)
        {
            return Result<Tenant>.NotFound();
        }

        // Update tenant properties
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            tenant.UpdateName(request.Name);
        }

        if (request.Description != null)
        {
            tenant.UpdateDescription(request.Description);
        }

        await tenantRepository.UpdateAsync(tenant, cancellationToken);
        await tenantRepository.SaveChangesAsync(cancellationToken);

        return Result<Tenant>.Success(tenant);
    }
}
