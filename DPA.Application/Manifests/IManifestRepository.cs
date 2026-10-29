using DPA.Domain.Manifests;

namespace DPA.Application.Manifests;

public interface IManifestRepository
{
    Task<IReadOnlyList<Manifest>> GetAllAsync(CancellationToken cancellationToken);
}