using DPA.Application.Manifests.GetManifests;
using DPA.Application.Shared.Messaging;

namespace DPA.Api.Endpoints;

public static class ManifestEndpoints
{
    public static IEndpointRouteBuilder MapManifestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/manifests");

        group.MapGet("/", GetManifests);

        return app;
    }

    private static async Task<IResult> GetManifests(ISender sender,
        CancellationToken cancellationToken)
    {
        var manifests = await sender.Send(new GetManifestsQuery(), 
            cancellationToken);
        return Results.Ok(manifests);
    }
}