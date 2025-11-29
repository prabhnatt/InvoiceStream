using System.Security.Cryptography;
using System.Text;
using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingCore.Services
{
    public class VerificationCodeService
    {
        private readonly MongoDbContext _db;

        public VerificationCodeService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateEmailVerificationCodeAsync(User user, CancellationToken ct = default)
        {
            const string purpose = "email_verification";

            //Simple 6-digit numeric code.
            var rawCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var now = DateTime.UtcNow;

            var code = new VerificationCode
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                Email = user.Email,
                Purpose = purpose,
                CodeHash = Hash(rawCode),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(15), // 15-minute validity
                Used = false
            };

            await _db.VerificationCodes.InsertOneAsync(code, cancellationToken: ct);

            //Return raw code so caller can send via email/SMS
            return rawCode;
        }

        public async Task<VerificationCode?> ValidateCodeAsync(
            string email,
            string purpose,
            string code,
            CancellationToken ct = default)
        {
            var emailNorm = NormalizeEmail(email);
            var codeHash = Hash(code);
            var now = DateTime.UtcNow;

            var filter = Builders<VerificationCode>.Filter.Where(v =>
                v.Email == emailNorm &&
                v.Purpose == purpose &&
                !v.Used &&
                v.ExpiresAt >= now);

            var candidates = await _db.VerificationCodes
                .Find(filter)
                .SortByDescending(v => v.CreatedAt)
                .ToListAsync(ct);

            var match = candidates.FirstOrDefault(v => v.CodeHash == codeHash);
            if (match is null)
                return null;

            // Mark used
            var update = Builders<VerificationCode>.Update
                .Set(v => v.Used, true)
                .Set(v => v.UsedAt, now);

            await _db.VerificationCodes.UpdateOneAsync(
                v => v.Id == match.Id, update, cancellationToken: ct);

            return match;
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}