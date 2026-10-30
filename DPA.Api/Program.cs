using DPA.Application;
using DPA.Application.Manifests.GetManifests;
using DPA.Application.Shared.Messaging;
using DPA.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Dpa")
                       ?? throw new InvalidOperationException("Connection string 'Dpa' is missing.");

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

var app = builder.Build();

app.MapGet("/manifests", async (ISender sender, CancellationToken cancellationToken) =>
{
    var manifests = await sender.Send(new GetManifestsQuery(), cancellationToken);
    return Results.Ok(manifests);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

