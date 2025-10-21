using Endatix.Core.Entities;
using Endatix.Core.Infrastructure.Messaging;
using Endatix.Core.Infrastructure.Result;

namespace Endatix.Core.UseCases.Tenants.List;

/// <summary>
/// Query for listing all tenants.
/// </summary>
public record ListTenantsQuery : IQuery<Result<IEnumerable<Tenant>>>;
