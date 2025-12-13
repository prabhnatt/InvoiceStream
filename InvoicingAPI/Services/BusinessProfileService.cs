using InvoicingCore;
using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingAPI.Services;

public class BusinessProfileService
{
    private readonly MongoDbContext _db;

    public BusinessProfileService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessProfile> GetOrCreateForUserAsync(string userId, CancellationToken ct = default)
    {
        var profile = await _db.BusinessProfiles
            .Find(x => x.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (profile is not null)
            return profile;

        profile = new BusinessProfile
        {
            UserId = userId,
            BusinessName = "Your Business Name",
            DefaultCurrency = "CAD",
            DefaultTaxRate = 0.13m,
            DefaultPaymentTermsDays = 14
        };

        await _db.BusinessProfiles.InsertOneAsync(profile, cancellationToken: ct);
        return profile;
    }

    public async Task<BusinessProfile> UpdateForUserAsync(string userId, BusinessProfile updated, CancellationToken ct = default)
    {
        updated.UserId = userId;

        await _db.BusinessProfiles.ReplaceOneAsync(
            x => x.UserId == userId,
            updated,
            new ReplaceOptions { IsUpsert = true },
            ct);

        return updated;
    }
}
