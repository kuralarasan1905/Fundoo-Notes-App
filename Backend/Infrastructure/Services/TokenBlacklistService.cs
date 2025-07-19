using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// In-memory implementation of token blacklist service
    /// For production, consider using Redis or database storage
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenBlacklistService> _logger;
        private readonly string _blacklistPrefix = "blacklist_";
        private readonly string _userTokensPrefix = "user_tokens_";

        public TokenBlacklistService(IMemoryCache cache, ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task BlacklistTokenAsync(string token, DateTime expiration)
        {
            try
            {
                var tokenId = GetTokenId(token);
                if (string.IsNullOrEmpty(tokenId))
                {
                    _logger.LogWarning("Cannot blacklist token: Unable to extract token ID");
                    return;
                }

                var cacheKey = _blacklistPrefix + tokenId;
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expiration,
                    Priority = CacheItemPriority.Low
                };

                _cache.Set(cacheKey, true, cacheOptions);
                _logger.LogInformation("Token blacklisted: {TokenId}", tokenId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token");
                throw;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            try
            {
                var tokenId = GetTokenId(token);
                if (string.IsNullOrEmpty(tokenId))
                {
                    return false;
                }

                var cacheKey = _blacklistPrefix + tokenId;
                var isBlacklisted = _cache.TryGetValue(cacheKey, out _);

                await Task.CompletedTask;
                return isBlacklisted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token blacklist status");
                return false;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            // In-memory cache automatically removes expired entries
            // This method is for compatibility with database implementations
            _logger.LogInformation("Memory cache automatically handles expired token cleanup");
            await Task.CompletedTask;
            return 0;
        }

        public async Task BlacklistUserTokensAsync(int userId)
        {
            try
            {
                // For in-memory implementation, we'll store a flag that all tokens for this user are invalid
                // In a real implementation, you'd query all active tokens for the user and blacklist them
                var cacheKey = _userTokensPrefix + userId;
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddDays(1), // Tokens typically expire within 24 hours
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, DateTime.UtcNow, cacheOptions);
                _logger.LogInformation("All tokens blacklisted for user: {UserId}", userId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting user tokens for user: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Check if all tokens for a user are blacklisted
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tokenIssuedAt">When the token was issued</param>
        /// <returns>True if user tokens are blacklisted</returns>
        public async Task<bool> AreUserTokensBlacklistedAsync(int userId, DateTime tokenIssuedAt)
        {
            try
            {
                var cacheKey = _userTokensPrefix + userId;
                if (_cache.TryGetValue(cacheKey, out var blacklistTime) && blacklistTime is DateTime blacklistDateTime)
                {
                    // If token was issued before the blacklist time, it's invalid
                    var isBlacklisted = tokenIssuedAt < blacklistDateTime;
                    await Task.CompletedTask;
                    return isBlacklisted;
                }

                await Task.CompletedTask;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user token blacklist status for user: {UserId}", userId);
                return false;
            }
        }

        private string? GetTokenId(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                {
                    return null;
                }

                var jsonToken = handler.ReadJwtToken(token);
                return jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting token ID from JWT");
                return null;
            }
        }
    }
}
