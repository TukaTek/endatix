using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Endatix.Api.Infrastructure;
using Endatix.Core.UseCases.Tenants.Create;
using Endatix.Core.Abstractions.Authorization;

namespace Endatix.Api.Endpoints.Tenants;

/// <summary>
/// Endpoint for creating a new tenant.
/// </summary>
public class Create(IMediator mediator) : Endpoint<CreateTenantRequest, Results<Created<CreateTenantResponse>, BadRequest>>
{
    /// <summary>
    /// Configures the endpoint settings.
    /// </summary>
    public override void Configure()
    {
        Post("tenants");
        Permissions(Actions.Platform.ManageTenants);
        Summary(s =>
        {
            s.Summary = "Create a new tenant";
            s.Description = "Creates a new tenant in the system. Admin only.";
            s.Responses[201] = "Tenant created successfully.";
            s.Responses[400] = "Invalid input data.";
            s.Responses[401] = "Unauthorized - authentication required.";
            s.Responses[403] = "Forbidden - admin access required.";
        });
    }

    /// <inheritdoc/>
    public override async Task<Results<Created<CreateTenantResponse>, BadRequest>> ExecuteAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var createTenantCommand = new CreateTenantCommand(request.Name!, request.Description);
        var result = await mediator.Send(createTenantCommand, cancellationToken);

        return TypedResultsBuilder
            .MapResult(result, TenantMapper.Map<CreateTenantResponse>)
            .SetTypedResults<Created<CreateTenantResponse>, BadRequest>();
    }
}
