using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.EI;
using Accounting.Utilities.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Accounting.Web.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemoryCacheController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheController()
        {
            _memoryCache = WebAppReferenceUtility._memoryCache;
        }

        /// <summary>
        /// Gets all memory cache keys.
        /// </summary>
        [HttpGet("GetAllKeys")]
        public IActionResult GetAllKeys()
        {
            try
            {
                FileLogger.LogInformation($"MemoryCacheController: GetAllKeys Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                var entries = GetCacheEntries();
                var keys = entries.Select(x => x.Key?.ToString()).Where(x => !string.IsNullOrEmpty(x)).ToList();
                return Ok(keys);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: GetAllKeys Exception: {ex} Message: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Gets memory cache data for the specified keys or all cache entries.
        /// </summary>
        [HttpPost("GetAllKeysData")]
        public IActionResult GetAllKeysData([FromQuery] bool getAllKeyData, [FromBody] List<string>? keys)
        {
            try
            {
                FileLogger.LogInformation("MemoryCacheController: GetAllKeysData Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                var entries = GetCacheEntries();
                var result = new Dictionary<string, object?>();

                // Get all cache data
                if (getAllKeyData)
                {
                    foreach (var entry in entries)
                    {
                        string? key = entry.Key?.ToString();
                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            result[key] = entry.Value;
                        }
                    }
                }
                else
                {
                    // Get data only for specified keys
                    if (keys == null || keys.Count == 0)
                    {
                        return BadRequest("Please provide at least one cache key when GetAllKeyData is false.");
                    }

                    foreach (string key in keys)
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            continue;
                        }

                        var entry = entries.FirstOrDefault(x => string.Equals(x.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase));
                        if (entry != null)
                        {
                            result[key] = entry.Value;
                        }
                    }
                }

                FileLogger.LogInformation($"MemoryCacheController: GetAllKeysData Completed. Count: {result.Count}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: GetAllKeysData Exception: {ex} Message: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Removes the specified memory cache keys.
        /// </summary>
        [HttpPost("RemoveKeys")]
        public IActionResult RemoveKeys([FromBody] List<string> keys)
        {
            try
            {
                FileLogger.LogInformation("MemoryCacheController: RemoveKeys Started.");

                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                if (keys == null || keys.Count == 0)
                {
                    return BadRequest("Please provide at least one cache key.");
                }

                var results = new List<object>();

                foreach (string key in keys)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        results.Add(new
                        {
                            Key = key,
                            Status = "Failed",
                            Message = "Cache key is empty or invalid."
                        });
                        continue;
                    }

                    // Check whether the cache key exists
                    if (_memoryCache.TryGetValue(key, out _))
                    {
                        // Remove the cache entry
                        _memoryCache.Remove(key);
                        results.Add(new
                        {
                            Key = key,
                            Status = "Success",
                            Message = "Cache data removed successfully."
                        });
                    }
                    else
                    {
                        results.Add(new
                        {
                            Key = key,
                            Status = "NotFound",
                            Message = "Cache key does not exist."
                        });
                    }
                }

                FileLogger.LogInformation($"MemoryCacheController: RemoveKeys Completed. Requested Keys: {keys.Count}");
                return Ok(results);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: RemoveKeys Exception: {ex} Message: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Removes all memory cache entries.
        /// </summary>
        [HttpPost("RemoveAllKeys")]
        public IActionResult RemoveAllKeys()
        {
            try
            {
                FileLogger.LogInformation("MemoryCacheController: RemoveAllKeys Started.");
                if (!Request.Headers.TryGetValue("Authorization", out var token))
                {
                    return Unauthorized("Authorization token missing.");
                }

                if (token != MasterLookup.ApplicationAuthToken)
                {
                    return Unauthorized("Invalid token.");
                }

                var entries = GetCacheEntries();
                var keys = entries?.Select(x => x.Key?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
                if (keys?.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        _memoryCache.Remove(key);
                    }

                    FileLogger.LogInformation($"MemoryCacheController: RemoveAllKeys Completed. Removed Count: {keys.Count}");
                }

                return Ok(new
                {
                    Message = keys?.Count > 0? "All memory cache data removed successfully." : "No memory cache data found.",
                    RemovedCount = keys?.Count
                });
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: RemoveAllKeys Exception: {ex} Message: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Gets the current MemoryCache entries.
        /// </summary>
        private List<CacheEntryInfo> GetCacheEntries()
        {
            try
            {
                var entries = new List<CacheEntryInfo>();
                if (_memoryCache is not MemoryCache memoryCache)
                    return entries;

                var coherentStateField = typeof(MemoryCache)
                    .GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);

                if (coherentStateField == null)
                    return entries;

                var coherentState = coherentStateField.GetValue(memoryCache);

                if (coherentState == null)
                    return entries;

                var entriesCollectionField = coherentState.GetType()
                    .GetField("_stringEntries", BindingFlags.NonPublic | BindingFlags.Instance);

                var entriesCollection = entriesCollectionField?.GetValue(coherentState)
                    as IDictionary;

                if (entriesCollection != null)
                {
                    foreach (DictionaryEntry item in entriesCollection)
                    {
                        entries.Add(new CacheEntryInfo
                        {
                            Key = item.Key,
                            Value = GetCacheValue(item.Value)
                        });
                    }
                }

                return entries;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: GetCacheEntries Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        private object? GetCacheValue(object? cacheEntry)
        {
            try
            {
                if (cacheEntry == null)
                    return null;

                var valueProperty = cacheEntry.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return valueProperty?.GetValue(cacheEntry);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MemoryCacheController: GetCacheValue Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        private class CacheEntryInfo
        {
            public object? Key { get; set; }
            public object? Value { get; set; }
        }
    }
}