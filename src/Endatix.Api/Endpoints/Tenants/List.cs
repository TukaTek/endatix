using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Endatix.Api.Infrastructure;
using Endatix.Core.UseCases.Tenants.List;
using Endatix.Core.Abstractions.Authorization;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Endpoint for listing all tenants.
/// </summary>
public class List(IMediator mediator) : EndpointWithoutRequest<Results<Ok<ListTenantsResponse>, BadRequest>>
{
    /// <summary>
    /// Configures the endpoint settings.
    /// </summary>
    public override void Configure()
    {
        Get("tenants");
        Permissions(Actions.Platform.ManageTenants);
        Summary(s =>
        {
            s.Summary = "List all tenants";
            s.Description = "Retrieves a list of all tenants in the system. Admin only.";
            s.Responses[200] = "List of tenants retrieved successfully.";
            s.Responses[400] = "Bad request.";
            s.Responses[401] = "Unauthorized - authentication required.";
            s.Responses[403] = "Forbidden - admin access required.";
        });
    }

    /// <inheritdoc/>
    public override async Task<Results<Ok<ListTenantsResponse>, BadRequest>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var query = new ListTenantsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return TypedResultsBuilder
            .MapResult(result, tenants => new ListTenantsResponse
            {
                Tenants = tenants.Select(TenantMapper.Map<TenantModel>).ToList()
            })
            .SetTypedResults<Ok<ListTenantsResponse>, BadRequest>();
    }
}
