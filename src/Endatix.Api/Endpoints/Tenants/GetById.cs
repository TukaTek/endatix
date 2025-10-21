using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Endatix.Api.Infrastructure;
using Endatix.Core.UseCases.Tenants.Get;
using Endatix.Infrastructure.Identity.Authorization;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Endpoint for getting a tenant by ID.
/// </summary>
public class GetById(IMediator mediator) : Endpoint<GetTenantRequest, Results<Ok<GetTenantResponse>, BadRequest, NotFound>>
{
    /// <summary>
    /// Configures the endpoint settings.
    /// </summary>
    public override void Configure()
    {
        Get("tenants/{id}");
        Permissions(Allow.AllowAll);
        Summary(s =>
        {
            s.Summary = "Get tenant by ID";
            s.Description = "Retrieves a specific tenant by its ID. Admin only.";
            s.Responses[200] = "Tenant retrieved successfully.";
            s.Responses[404] = "Tenant not found.";
            s.Responses[400] = "Invalid tenant ID.";
            s.Responses[401] = "Unauthorized - authentication required.";
            s.Responses[403] = "Forbidden - admin access required.";
        });
    }

    /// <inheritdoc/>
    public override async Task<Results<Ok<GetTenantResponse>, BadRequest, NotFound>> ExecuteAsync(GetTenantRequest request, CancellationToken cancellationToken)
    {
        var query = new GetTenantQuery(request.Id);
        var result = await mediator.Send(query, cancellationToken);

        return TypedResultsBuilder
            .MapResult(result, TenantMapper.Map<GetTenantResponse>)
            .SetTypedResults<Ok<GetTenantResponse>, BadRequest, NotFound>();
    }
}
