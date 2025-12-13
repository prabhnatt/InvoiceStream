using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingCore.Services
{
    public class UserService
    {
        private readonly MongoDbContext _db;

        public UserService(MongoDbContext db)
        {
            _db = db;
        }
        //Google; Apple to be added
        public async Task<User> GetOrCreateFromExternalAsync(
            string provider,
            string subject,
            string? email,
            string? displayName,
            CancellationToken ct = default)
        {
            //provider + subject uniquely identify an external account
            var filter = Builders<User>.Filter.ElemMatch(
                u => u.Identities,
                i => i.Provider == provider && i.Subject == subject);

            var now = DateTime.UtcNow;
            var existing = await _db.Users.Find(filter).FirstOrDefaultAsync(ct);

            if (existing is not null)
            {
                existing.LastLoginAt = now;

                var identity = existing.Identities.First(i => i.Provider == provider && i.Subject == subject);
                identity.LastUsedAt = now;

                if (!string.IsNullOrWhiteSpace(email))
                {
                    identity.Email ??= email;
                    existing.Email = email; //keep primary email current
                }

                if (!string.IsNullOrWhiteSpace(displayName))
                    existing.DisplayName ??= displayName;

                await _db.Users.ReplaceOneAsync(u => u.Id == existing.Id, existing, cancellationToken: ct);

                return existing;
            }

            //Create new user
            var userId = $"usr_{Guid.NewGuid():N}";

            var user = new User
            {
                Id = userId,
                Email = email ?? $"{provider}:{subject}",
                DisplayName = displayName ?? email,
                CreatedAt = now,
                LastLoginAt = now,
                Identities = new List<UserIdentity>
            {
                new()
                {
                    Provider = provider,
                    Subject = subject,
                    Email = email,
                    LastUsedAt = now
                }
            },
                Settings = new UserSettings()
            };

            await _db.Users.InsertOneAsync(user, cancellationToken: ct);

            return user;
        }

        //Local auth (email/password)

        public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
        {
            email = NormalizeEmail(email);
            return await _db.Users.Find(u => u.Email == email).FirstOrDefaultAsync(ct);
        }

        public async Task<User> CreateLocalUserAsync(
            string email,
            string password,
            string? displayName,
            CancellationToken ct = default)
        {
            email = NormalizeEmail(email);

            var existing = await FindByEmailAsync(email, ct);
            if (existing is not null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var now = DateTime.UtcNow;
            var userId = $"usr_{Guid.NewGuid():N}";

            //bcrypt hash (salt + work factor embedded in result)
            var workFactor = 12;
            var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);

            var user = new User
            {
                Id = userId,
                Email = email,
                DisplayName = displayName ?? email,
                CreatedAt = now,
                LastLoginAt = now,
                EmailVerified = false,
                LocalAuth = new LocalAuthInfo
                {
                    PasswordHash = hash,
                    Algorithm = "bcrypt",
                    WorkFactor = workFactor
                },
                Settings = new UserSettings()
            };

            await _db.Users.InsertOneAsync(user, cancellationToken: ct);

            return user;
        }

        public async Task<User?> ValidateLocalUserAsync(
            string email,
            string password,
            CancellationToken ct = default)
        {
            email = NormalizeEmail(email);

            var user = await FindByEmailAsync(email, ct);
            if (user?.LocalAuth is null)
                return null;

            var hash = user.LocalAuth.PasswordHash;

            //bcrypt verify (checks embedded salt)
            var ok = BCrypt.Net.BCrypt.Verify(password, hash);
            if (!ok)
                return null;

            //update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _db.Users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);

            return user;
        }

        //Email Verification
        public async Task MarkEmailVerifiedAsync(string userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var update = Builders<User>.Update
                .Set(u => u.EmailVerified, true)
                .Set(u => u.EmailVerifiedAt, now);

            await _db.Users.UpdateOneAsync(u => u.Id == userId, update, cancellationToken: ct);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
