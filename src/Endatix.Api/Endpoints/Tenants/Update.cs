using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Endatix.Api.Infrastructure;
using Endatix.Core.UseCases.Tenants.Update;
using Endatix.Core.Abstractions.Authorization;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Endpoint for updating a tenant.
/// </summary>
public class Update(IMediator mediator) : Endpoint<UpdateTenantRequest, Results<Ok<UpdateTenantResponse>, BadRequest, NotFound>>
{
    /// <summary>
    /// Configures the endpoint settings.
    /// </summary>
    public override void Configure()
    {
        Put("tenants/{id}");
        Permissions(Actions.Platform.ManageTenants);
        Summary(s =>
        {
            s.Summary = "Update a tenant";
            s.Description = "Updates an existing tenant's information. Admin only.";
            s.Responses[200] = "Tenant updated successfully.";
            s.Responses[404] = "Tenant not found.";
            s.Responses[400] = "Invalid input data.";
            s.Responses[401] = "Unauthorized - authentication required.";
            s.Responses[403] = "Forbidden - admin access required.";
        });
    }

    /// <inheritdoc/>
    public override async Task<Results<Ok<UpdateTenantResponse>, BadRequest, NotFound>> ExecuteAsync(UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        var updateTenantCommand = new UpdateTenantCommand(request.Id, request.Name, request.Description);
        var result = await mediator.Send(updateTenantCommand, cancellationToken);

        return TypedResultsBuilder
            .MapResult(result, TenantMapper.Map<UpdateTenantResponse>)
            .SetTypedResults<Ok<UpdateTenantResponse>, BadRequest, NotFound>();
    }
}
