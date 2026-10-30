namespace DPA.Application.Manifests.GetManifests;

public record ManifestDto(
    Guid Id,
    Guid DestinationId,
    Guid? VehicleId,
    string Status,
    DateTimeOffset RequiredArrivalAt,
    int PlacedPallets);