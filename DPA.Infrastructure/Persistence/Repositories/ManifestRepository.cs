using DPA.Application.Manifests;
using DPA.Domain.Manifests;
using Microsoft.EntityFrameworkCore;

namespace DPA.Infrastructure.Persistence.Repositories;

public class ManifestRepository : IManifestRepository
{
    private readonly DpaDbContext _context;
    
    public ManifestRepository(DpaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Manifest>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Manifests
            .AsNoTracking()
            .OrderBy(m => m.RequiredArrivalAt)
            .ToListAsync(cancellationToken);
    }
}