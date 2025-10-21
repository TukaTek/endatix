using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Domain;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Delete;

/// <summary>
/// Handler for deleting a tenant (soft delete).
/// </summary>
public class DeleteTenantHandler(IRepository<Tenant> tenantRepository)
    : ICommandHandler<DeleteTenantCommand, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant == null)
        {
            return Result<Tenant>.NotFound();
        }

        // Soft delete using the base entity Delete method
        tenant.Delete();

        await tenantRepository.UpdateAsync(tenant, cancellationToken);
        await tenantRepository.SaveChangesAsync(cancellationToken);

        return Result<Tenant>.Success(tenant);
    }
}
