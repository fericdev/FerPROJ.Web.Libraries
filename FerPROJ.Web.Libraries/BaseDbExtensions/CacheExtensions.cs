using FerPROJ.Web.Libraries.BaseAssemblies;
using FerPROJ.Web.Libraries.BaseDataExtensions;
using FerPROJ.Web.Libraries.BaseDbHelper;
using FerPROJ.Web.Libraries.BaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDbExtensions {
    public static class CacheExtensions {
        #region Fields
        private static readonly MemoryCache _cache;
        private static readonly ConcurrentDictionary<string, bool> _cacheKeys;
        #endregion

        #region ctor
        static CacheExtensions() {
            _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            _cacheKeys = new ConcurrentDictionary<string, bool>();
        }
        #endregion

        #region Save
        public async static Task SaveToCacheAsync<TEntity>(TEntity value) where TEntity : class {
            if (value == null) {
                return;
            }

            string key = typeof(TEntity).Name;

            // Get the current cached list of TEntity
            var existingList = await GetAllListCacheAsync<TEntity>();

            // If there's an existing list, add the new value to it
            if (existingList == null) {
                existingList = new List<TEntity>();
            }
            else {
                var primaryKey = typeof(TEntity).GetPropertyInfo("Id");
                if (primaryKey == null) {
                    return;
                }

                var primaryValue = primaryKey.GetValue(value);
                var existingValue = existingList.FirstOrDefault(x => primaryKey.GetValue(x).Equals(primaryValue) == true);
                // Remove the existing value after identifying it
                if (existingValue != null) {
                    existingList = existingList.Where(x => !primaryKey.GetValue(x).Equals(primaryValue)).ToList();
                    ClearCache(existingValue.GetPropertyValue<string>("Id"));
                }
            }

            // Now you can safely add the new value
            existingList.Add(value);

            // Save the updated list to the cache
            _cache.Set(key, existingList, DateTimeOffset.MaxValue);

            ClearCacheByPrefix("GetEntity");
            ClearCacheByPrefix("GetList");
            ClearCacheByPrefix("GetItems");

            Console.WriteLine($"Cache Cleared and Saved: {key} TIME: {DateTime.Now.TimeOfDay} Count: {existingList.Count}");
        }
        public async static Task SaveAllToCacheAsync<TEntity>(List<TEntity> values) where TEntity : class {

            if (values.Count <= 0) {
                return;
            }

            string key = typeof(TEntity).Name;

            // Get the current cached list of TEntity
            var existingList = await GetAllListCacheAsync<TEntity>();

            // If there's no existing list, create a new one
            if (existingList == null) {
                existingList = new List<TEntity>();
            }
            else {

                var primaryKey = typeof(TEntity).GetPropertyInfo("Id");

                if (primaryKey == null) {
                    return;
                }

                // Prepare a list to store items to remove
                var itemsToRemove = new List<TEntity>();

                // Identify the items to remove without modifying the collection during iteration
                foreach (var value in values) {

                    var primaryValue = primaryKey.GetValue(value);

                    var existingValue = existingList.FirstOrDefault(x => primaryKey.GetValue(x).Equals(primaryValue));

                    if (existingValue != null) {

                        itemsToRemove.Add(existingValue);

                    }
                }

                // Remove identified items after the iteration
                foreach (var item in itemsToRemove) {

                    existingList.Remove(item);

                    ClearCache(item.GetPropertyValue<string>("Id"));
                }
            }

            // Add the new values to the existing list
            existingList.AddRange(values);

            // Save the updated list to the cache
            _cache.Set(key, existingList, DateTimeOffset.MaxValue);

            Console.WriteLine($"Cache Cleared and Saved: {key} TIME: {DateTime.Now.TimeOfDay} Count: {existingList.Count}");
        }

        public async static Task SaveAllToCacheAsync<TEntity>(IEnumerable<TEntity> values) where TEntity : class {

            if (values == null || !values.Any()) {

                return;

            }

            await SaveAllToCacheAsync(values.ToList());
        }

        public async static Task SaveAllToCacheAsync<TEntity>(ICollection<TEntity> values) where TEntity : class {

            if (values == null || !values.Any()) {

                return;

            }

            await SaveAllToCacheAsync(values.ToList());
        }

        #endregion

        #region Clear and Save
        public async static Task ClearAndSaveAllToCacheAsync<TEntity>(List<TEntity> values) where TEntity : class {
            //
            if (values.Count <= 0) {
                return;
            }
            //
            string key = typeof(TEntity).Name;

            // Clear the cache synchronously (fast operation)
            _cache.Remove(key);

            // Save the updated list to the cache synchronously
            _cache.Set(key, values, DateTimeOffset.MaxValue);

            Console.WriteLine($"Cache Cleared and Saved: {key} TIME: {DateTime.Now.TimeOfDay} Count: {values.Count}");

            await Task.CompletedTask;
        }

        public async static Task ClearAndSaveAllToCacheAsync<TEntity>(IEnumerable<TEntity> values) where TEntity : class {
            await ClearAndSaveAllToCacheAsync(values.ToList());
        }

        public async static Task ClearAndSaveAllToCacheAsync<TEntity>(ICollection<TEntity> values) where TEntity : class {
            await ClearAndSaveAllToCacheAsync(values.ToList());
        }

        #endregion

        #region Remove
        public async static Task RemoveFromCacheAsync<TEntity>(TEntity value) where TEntity : class {
            string key = typeof(TEntity).Name;

            // Get the current cached list of TEntity
            var existingList = await GetAllListCacheAsync<TEntity>();

            // If there's no existing list, there's nothing to remove
            if (existingList == null) {
                return;
            }
            else {
                var primaryKey = typeof(TEntity).GetPropertyInfo("Id");

                if (primaryKey == null) {
                    return;
                }

                var primaryValue = primaryKey.GetValue(value);
                var existingValue = existingList.FirstOrDefault(x => primaryKey.GetValue(x).Equals(primaryValue) == true);
                if (existingValue != null) {
                    existingList.Remove(existingValue);
                }

            }

            // Save the updated list to the cache
            _cache.Set(key, existingList, DateTimeOffset.MaxValue);

            ClearCache(value.GetPropertyValue<string>("Id"));
            ClearCacheByPrefix("GetEntity");
            ClearCacheByPrefix("GetList");
            ClearCacheByPrefix("GetItems");

            await Task.CompletedTask;
        }

        public async static Task RemoveAllFromCacheAsync<TEntity>(List<TEntity> values) where TEntity : class {

            if (values.Count <= 0) {
                return;
            }

            string key = typeof(TEntity).Name;

            // Get the current cached list of TEntity
            var existingList = await GetAllListCacheAsync<TEntity>();

            // If there's no existing list, return
            if (existingList == null) {
                return;
            }
            else {
                foreach (var value in values) {
                    existingList.Remove(value);
                    ClearCache(value.GetPropertyValue<string>("Id"));
                    ClearCacheByPrefix("GetEntity");
                    ClearCacheByPrefix("GetList");
                    ClearCacheByPrefix("GetItems");
                }
            }

            // Save the updated list to the cache
            _cache.Set(key, existingList, DateTimeOffset.MaxValue);

            await Task.CompletedTask;
        }


        public async static Task RemoveAllFromCacheAsync<TEntity>(IEnumerable<TEntity> values) where TEntity : class {
            await RemoveAllFromCacheAsync(values.ToList());
        }


        public async static Task RemoveAllFromCacheAsync<TEntity>(ICollection<TEntity> values) where TEntity : class {
            await RemoveAllFromCacheAsync(values.ToList());
        }

        #endregion

        #region Remove by list ids
        public async static Task RemoveAllByIdsFromCacheAsync<TEntity>(List<object> values) where TEntity : class {

            if (values.Count <= 0) {
                return;
            }

            string key = typeof(TEntity).Name;

            // Get the current cached list of TEntity
            var existingList = await GetAllListCacheAsync<TEntity>();

            // If there's no existing list, return
            if (existingList == null) {
                return;
            }

            var primaryKey = typeof(TEntity).GetPropertyInfo("Id");

            if (primaryKey == null) {
                return;
            }

            // Prepare a list to store items to remove
            var itemsToRemove = new List<TEntity>();

            // Loop through each object value in the values list
            foreach (var value in values) {

                var existingValue = existingList.FirstOrDefault(x => primaryKey.GetValue(x).Equals(value));

                if (existingValue != null) {

                    itemsToRemove.Add(existingValue);

                }

            }

            // Remove identified items after the iteration
            foreach (var item in itemsToRemove) {

                existingList.Remove(item);

            }

            // Save the updated list to the cache
            _cache.Set(key, existingList, DateTimeOffset.MaxValue);

            Console.WriteLine($"Updated: {key} TIME: {DateTime.Now.TimeOfDay} Count: {existingList.Count}");
        }

        // Overload for IEnumerable
        public async static Task RemoveAllFromCacheAsync<TEntity>(IEnumerable<object> values) where TEntity : class {
            await RemoveAllByIdsFromCacheAsync<TEntity>(values.ToList());
        }

        // Overload for ICollection
        public async static Task RemoveAllFromCacheAsync<TEntity>(ICollection<object> values) where TEntity : class {
            await RemoveAllByIdsFromCacheAsync<TEntity>(values.ToList());
        }
        #endregion

        #region Get 
        public async static Task<List<TEntity>> GetAllListCacheAsync<TEntity>() where TEntity : class {
            string key = typeof(TEntity).Name;
            return await Task.FromResult(_cache.Get(key) as List<TEntity>);
        }
        public async static Task<IEnumerable<TEntity>> GetAllEnumerableCacheAsync<TEntity>() where TEntity : class {
            var result = await GetAllListCacheAsync<TEntity>();
            return result?.AsEnumerable();
        }
        public async static Task<IQueryable<TEntity>> GetAllQueryableCacheAsync<TEntity>() where TEntity : class {
            var result = await GetAllListCacheAsync<TEntity>();
            return result?.AsQueryable();
        }
        public async static Task<TEntity> GetCacheByPredicateAsync<TEntity>(Func<TEntity, bool> predicate) where TEntity : class {

            var list = await GetAllListCacheAsync<TEntity>();

            return list?.FirstOrDefault(predicate);

        }
        #endregion

        #region Get or Create
        public static async Task<TResult> GetOrCreateCacheAsync<TResult>(object key, Func<Task<TResult>> createFunc) {
            // Try to get the value from cache
            var cachedValue = _cache.Get(key.ToString());

            // If found in cache, return it
            if (cachedValue != null) {
                return (TResult)cachedValue;
            }

            // If not in cache, create it using the provided function
            var newValue = await createFunc();

            // Store the new value in cache
            SetCache(key.ToString(), newValue);

            return newValue;
        }
        public static void SetCache(string key, object value) {
            _cache.Set(key, value, DateTimeOffset.MaxValue);
            _cacheKeys.TryAdd(key, true);
        }

        public static void ClearCache(string key) {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
        public static void ClearCacheByPrefix(string prefix) {
            var keysToRemove = _cacheKeys.Keys
                .Where(k => k.StartsWith(prefix))
                .ToList();

            foreach (var key in keysToRemove) {
                ClearCache(key);
            }
        }
        #endregion

        #region Load all Cached From DB
        public static List<Func<Task>> GetCacheMethodTasks<TContext>(params string[] repositoryAssemblies) where TContext : BaseDbContext {

            // Base generic
            var baseGenericType = typeof(BaseRepository<,,>);

            if (repositoryAssemblies.Length == 0) {
                var appName = AppDomain.CurrentDomain.FriendlyName;
                repositoryAssemblies = [$"{appName}.Repository"];
            }

            // Get the derived types (that implement BaseDBEntityAsync)
            var derivedTypes = baseGenericType.GetDerivedTypes(repositoryAssemblies);

            var tasks = new List<Func<Task>>();

            foreach (var type in derivedTypes) {

                var method = type.GetMethod("LoadCachedAsync");

                if (method != null) {

                    // Create a new instance of DbContext dynamically inside each task
                    tasks.Add(async () => {

                        var factory = BaseServiceManager.Get<IDbContextFactory<TContext>>.Service();

                        await using var freshDbContext = factory.CreateDbContext();

                        var instance = Activator.CreateInstance(type, freshDbContext);

                        await (Task)method.Invoke(instance, null);
                    });
                }
            }

            return tasks;
        }
        #endregion
    }
}
