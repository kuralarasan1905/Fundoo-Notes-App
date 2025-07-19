using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// High-performance caching service with intelligent cache management
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
        void InvalidateUserCache(int userId);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        private readonly ConcurrentHashSet<string> _cacheKeys;

        // Cache key prefixes for different data types
        private const string USER_NOTES_PREFIX = "user_notes_";
        private const string USER_LABELS_PREFIX = "user_labels_";
        private const string NOTE_PREFIX = "note_";
        private const string TEMPLATE_PREFIX = "template_";
        private const string COLORS_KEY = "available_colors";
        private const string PATTERNS_KEY = "available_patterns";

        // Default cache durations
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan StaticDataExpiration = TimeSpan.FromHours(1);
        private static readonly TimeSpan UserDataExpiration = TimeSpan.FromMinutes(10);

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _cacheKeys = new ConcurrentHashSet<string>();
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is T typedValue)
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                    return typedValue;
                }

                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {CacheKey}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var cacheExpiration = expiration ?? GetDefaultExpiration(key);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration,
                    Priority = GetCachePriority(key),
                    Size = EstimateSize(value)
                };

                _cache.Set(key, value, cacheOptions);
                _cacheKeys.Add(key);

                _logger.LogDebug("Cached value for key: {CacheKey} with expiration: {Expiration}", key, cacheExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {CacheKey}", key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Use semaphore to prevent multiple threads from executing the factory for the same key
            var lockKey = $"lock_{key}";
            var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check pattern: another thread might have populated the cache
                cachedValue = await GetAsync<T>(key, cancellationToken);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // Execute factory and cache the result
                _logger.LogDebug("Executing factory for cache key: {CacheKey}", key);
                var value = await factory();
                
                if (value != null)
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }

                return value;
            }
            finally
            {
                semaphore.Release();
                // Clean up the lock if no other threads are waiting
                if (semaphore.CurrentCount == 1)
                {
                    _locks.TryRemove(lockKey, out _);
                    semaphore.Dispose();
                }
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                _cache.Remove(key);
                _cacheKeys.TryRemove(key);
                _logger.LogDebug("Removed cache key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {CacheKey}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var keysToRemove = _cacheKeys.Where(key => key.Contains(pattern)).ToList();
                
                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key, cancellationToken);
                }

                _logger.LogDebug("Removed {Count} cache keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache keys by pattern: {Pattern}", pattern);
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var keysToRemove = _cacheKeys.ToList();
                
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }

                _cacheKeys.Clear();
                _logger.LogInformation("Cleared all cache entries: {Count} keys removed", keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }

        public void InvalidateUserCache(int userId)
        {
            try
            {
                var userPattern = $"user_{userId}_";
                var keysToRemove = _cacheKeys.Where(key => key.Contains(userPattern)).ToList();
                
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _cacheKeys.TryRemove(key);
                }

                _logger.LogDebug("Invalidated cache for user {UserId}: {Count} keys removed", userId, keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating user cache for user: {UserId}", userId);
            }
        }

        private TimeSpan GetDefaultExpiration(string key)
        {
            if (key.Contains(COLORS_KEY) || key.Contains(PATTERNS_KEY))
                return StaticDataExpiration;
            
            if (key.Contains(USER_NOTES_PREFIX) || key.Contains(USER_LABELS_PREFIX))
                return UserDataExpiration;
            
            return DefaultExpiration;
        }

        private CacheItemPriority GetCachePriority(string key)
        {
            if (key.Contains(COLORS_KEY) || key.Contains(PATTERNS_KEY))
                return CacheItemPriority.High;
            
            if (key.Contains(USER_NOTES_PREFIX))
                return CacheItemPriority.Normal;
            
            return CacheItemPriority.Low;
        }

        private long EstimateSize<T>(T value)
        {
            // Simple size estimation - in production, you might want more sophisticated sizing
            if (value is string str)
                return str.Length * 2; // Unicode characters are 2 bytes
            
            if (value is System.Collections.ICollection collection)
                return collection.Count * 100; // Rough estimate
            
            return 1000; // Default size estimate
        }

        // Helper method to generate cache keys
        public static string GenerateUserNotesKey(int userId, bool includeArchived, bool includeTrashed)
        {
            return $"{USER_NOTES_PREFIX}{userId}_{includeArchived}_{includeTrashed}";
        }

        public static string GenerateUserLabelsKey(int userId)
        {
            return $"{USER_LABELS_PREFIX}{userId}";
        }

        public static string GenerateNoteKey(int noteId)
        {
            return $"{NOTE_PREFIX}{noteId}";
        }
    }

    // Thread-safe HashSet implementation
    public class ConcurrentHashSet<T> : IDisposable
    {
        private readonly HashSet<T> _hashSet = new HashSet<T>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryRemove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Where(predicate).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<T> ToList()
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
