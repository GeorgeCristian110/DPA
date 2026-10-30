using DPA.Application.Shared.Messaging;

namespace DPA.Application.Manifests.GetManifests;

public class GetManifestsQueryHandler : IRequestHandler<GetManifestsQuery, IReadOnlyList<ManifestDto>>
{
    private readonly IManifestRepository _repository;

    public GetManifestsQueryHandler(IManifestRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ManifestDto>> Handle(GetManifestsQuery request,
        CancellationToken cancellationToken)
    {
        var manifests = await _repository.GetAllAsync(cancellationToken);

        return manifests
            .Select(m => new ManifestDto(
                m.Id,
                m.DestinationId,
                m.VehicleId,
                m.Status.ToString(),
                m.RequiredArrivalAt,
                m.Placements.Count))
            .ToList();
    }
}