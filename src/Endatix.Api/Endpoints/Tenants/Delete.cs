using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Endatix.Api.Infrastructure;
using Endatix.Core.UseCases.Tenants.Delete;
using Endatix.Infrastructure.Identity.Authorization;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Endpoint for deleting a tenant (soft delete).
/// </summary>
public class Delete(IMediator mediator) : Endpoint<DeleteTenantRequest, Results<Ok<string>, BadRequest, NotFound>>
{
    /// <summary>
    /// Configures the endpoint settings.
    /// </summary>
    public override void Configure()
    {
        Delete("tenants/{id}");
        Permissions(Allow.AllowAll);
        Summary(s =>
        {
            s.Summary = "Delete a tenant";
            s.Description = "Soft deletes a tenant from the system. Admin only.";
            s.Responses[204] = "Tenant deleted successfully.";
            s.Responses[404] = "Tenant not found.";
            s.Responses[400] = "Invalid tenant ID.";
            s.Responses[401] = "Unauthorized - authentication required.";
            s.Responses[403] = "Forbidden - admin access required.";
        });
    }

    /// <inheritdoc/>
    public override async Task<Results<Ok<string>, BadRequest, NotFound>> ExecuteAsync(DeleteTenantRequest request, CancellationToken cancellationToken)
    {
        var deleteTenantCommand = new DeleteTenantCommand(request.Id);
        var result = await mediator.Send(deleteTenantCommand, cancellationToken);

        return TypedResultsBuilder
            .MapResult(result, tenant => tenant.Id.ToString())
            .SetTypedResults<Ok<string>, BadRequest, NotFound>();
    }
}
