using Ardalis.GuardClauses;
using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.Create;

/// <summary>
/// Command for creating a tenant.
/// </summary>
public record CreateTenantCommand : ICommand<Result<Tenant>>
{
    public string Name { get; init; }
    public string? Description { get; init; }

    public CreateTenantCommand(string name, string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}
