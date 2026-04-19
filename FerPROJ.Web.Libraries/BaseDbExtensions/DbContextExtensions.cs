using FerPROJ.Web.Libraries.BaseDbExtensions;
using FerPROJ.Web.Libraries.BaseModels;
using FerPROJ.Web.Libraries.BaseDataHelper;
using FerPROJ.Web.Libraries.BaseDataExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FerPROJ.Web.Libraries.BaseEntities;
using FerPROJ.Libraries.DBHelper.Extensions;

namespace FerPROJ.Web.Libraries.BaseDbExtensions {
    public static class DbContextExtensions {

        #region Remove
        public static async Task RemoveRangeAndCommitAsync<TEntity>(this DbContext context, ICollection<TEntity> entity) where TEntity : class {
            if (entity.Count() <= 0) {
                return;
            }
            context.Set<TEntity>().RemoveRange(entity);
            await context.SaveChangesAsync();
            await CacheExtensions.RemoveAllFromCacheAsync(entity);
        }
        public static async Task RemoveRangeAndCommitAsync<TEntity>(this DbContext context, IEnumerable<TEntity> entity) where TEntity : class {
            if (entity.Count() <= 0) {
                return;
            }
            context.Set<TEntity>().RemoveRange(entity);
            await context.SaveChangesAsync();
            await CacheExtensions.RemoveAllFromCacheAsync(entity);
        }
        public static async Task RemoveRangeAsync<TEntity>(this DbContext context, ICollection<TEntity> entity) where TEntity : class {
            if (entity.Count() <= 0) {
                return;
            }
            context.Set<TEntity>().RemoveRange(entity);
            await CacheExtensions.RemoveFromCacheAsync(entity);
        }
        public static async Task RemoveRangeAsync<TEntity>(this DbContext context, IEnumerable<TEntity> entity) where TEntity : class {
            if (entity.Count() <= 0) {
                return;
            }
            context.Set<TEntity>().RemoveRange(entity);
            await CacheExtensions.RemoveAllFromCacheAsync(entity);
        }
        public static async Task RemoveAndCommitAsync<TEntity>(this DbContext context, TEntity entity) where TEntity : class {

            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();
            await CacheExtensions.RemoveFromCacheAsync(entity);
        }
        public static async Task RemoveAsync<TEntity>(this DbContext context, TEntity entity) where TEntity : class {

            context.Set<TEntity>().Remove(entity);
            await CacheExtensions.RemoveFromCacheAsync(entity);

        }
        #endregion

        #region Soft Remove
        public static async Task SoftRemoveAndCommitAsync<TEntity>(this DbContext context, TEntity entity) where TEntity : BaseEntity {
            await context.UpdateAndCommitAsync(entity);
        }
        #endregion

        #region Update
        public static async Task UpdateAsync<TEntity>(
             this DbContext context,
             TEntity entity)
             where TEntity : class {

            context.Set<TEntity>().Update(entity);

            await CacheExtensions.SaveToCacheAsync(entity);
        }
        public static async Task UpdateRangeAsync<TEntity>(
             this DbContext context,
             List<TEntity> entity)
             where TEntity : class {
            foreach (var item in entity) {
                await context.UpdateAsync(item);
            }
        }
        public static async Task UpdateAndCommitAsync<TEntity>(
             this DbContext context,
             TEntity entity)
             where TEntity : class {

            await context.UpdateAsync(entity);

            await context.SaveChangesAsync();
        }
        public static async Task UpdateRangeAndCommitAsync<TEntity>(
             this DbContext context,
             List<TEntity> entity)
             where TEntity : class {

            foreach (var item in entity) {
                await context.UpdateAsync(item);
            }
            await context.SaveChangesAsync();
        }
        #endregion

        #region Save
        public static async Task SaveAsync<TEntity>(
             this DbContext context,
             TEntity entity)
             where TEntity : class {

            context.Set<TEntity>().Add(entity);

            await CacheExtensions.SaveToCacheAsync(entity);
        }
        public static async Task SaveRangeAsync<TEntity>(
             this DbContext context,
             ICollection<TEntity> entity)
             where TEntity : class {

            context.Set<TEntity>().AddRange(entity);

            await CacheExtensions.SaveAllToCacheAsync(entity);

        }
        public static async Task SaveAndCommitAsync<TEntity>(
             this DbContext context,
             TEntity entity)
             where TEntity : class {

            context.Set<TEntity>().Add(entity);

            await CacheExtensions.SaveToCacheAsync(entity);

            await context.SaveChangesAsync();
        }
        public static async Task SaveRangeAndCommitAsync<TEntity>(
             this DbContext context,
             List<TEntity> entity)
             where TEntity : class {

            context.Set<TEntity>().AddRange(entity);

            await CacheExtensions.SaveAllToCacheAsync(entity);

            await context.SaveChangesAsync();
        }
        // DTO
        public static async Task SaveModelAsync<TSource, TEntity>(this DbContext context, TSource model) where TSource : BaseModel where TEntity : class {

            var tbl = new CMappingExtension<TSource, TEntity>().GetMappingResult(model);

            await context.SaveAsync(tbl);

        }
        public static async Task SaveModelAndCommitAsync<TSource, TEntity>(this DbContext context, TSource model) where TSource : BaseModel where TEntity : class {
            var tbl = new CMappingExtension<TSource, TEntity>().GetMappingResult(model);
            await context.SaveAndCommitAsync(tbl);
        }

        #endregion

        #region Update DTO
        public static async Task UpdateModelAsync<TSource, TEntity>(this DbContext context, TSource model) where TSource : BaseModel where TEntity : class {

            var tbl = new CMappingExtension<TSource, TEntity>().GetMappingResult(model);

            context.Set<TEntity>().Update(tbl);

            await CacheExtensions.SaveToCacheAsync(tbl);

        }
        public static async Task UpdateModelAndCommitAsync<TSource, TEntity>(this DbContext context, TSource model) where TSource : BaseModel where TEntity : class {

            var tbl = new CMappingExtension<TSource, TEntity>().GetMappingResult(model);

            context.Set<TEntity>().Update(tbl);

            await CacheExtensions.SaveToCacheAsync(tbl);

            await context.SaveChangesAsync();
        }
        #endregion

        #region Get Method
        public static async Task<TEntity> GetByIdAsync<TEntity>(this DbContext context, Guid id) where TEntity : BaseEntity {
            return await context.GetByPredicateAsync<TEntity>(c => c.Id == id);
        }
        public static async Task<TEntity> GetByIdAsync<TEntity, TType>(this DbContext context, TType id) where TEntity : class {

            // Get Primary Key
            PropertyInfo keyProperty = typeof(TEntity).GetPropertyInfo("Id");

            // Create a parameter expression for the entity type (e.g., "e => e.Id == id")
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            // Ensure type compatibility by converting `id` to the primary key's type
            var idConstant = Expression.Constant(Convert.ChangeType(id, keyProperty.PropertyType), keyProperty.PropertyType);

            // Create the equality expression (e.g., "e.Id == id")
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, keyProperty.Name),
                    idConstant
                ),
                parameter
            );

            return await context.GetByPredicateAsync(predicate);

        }
        public static async Task<TEntity> GetByIdAsync<TEntity, TType>(
            this DbContext context,
            TType id,
            string propertyName) where TEntity : class {

            // Find the specified property on TEntity
            var property = typeof(TEntity).GetProperty(propertyName);
            if (property == null) {
                throw new ArgumentException($"Property '{propertyName}' does not exist on entity {typeof(TEntity).Name}");
            }

            // Create a parameter expression for the entity type
            var parameter = Expression.Parameter(typeof(TEntity), "e");

            // Create the equality expression for the specified property
            var predicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, property),
                    Expression.Constant(id, typeof(TType))
                ),
                parameter
            );

            return await context.GetByPredicateAsync(predicate);

        }
        public static async Task<TEntity> GetCacheByPredicateAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate) where TEntity : class {

            var cachedData = await CacheExtensions.GetAllQueryableCacheAsync<TEntity>();

            if (cachedData != null && cachedData.Any()) {

                return cachedData.FirstOrDefault(predicate);

            }

            return null;

        }
        public static async Task<TEntity> GetByPredicateAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate) where TEntity : class {

            return await context.Set<TEntity>().FirstOrDefaultAsync(predicate);

        }
        public static async Task<int> GetCountAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate = null) where TEntity : class {
            if (predicate != null) {
                return await context.Set<TEntity>().CountAsync(predicate);
            }
            return await context.Set<TEntity>().CountAsync();
        }
        public static async Task<string> GetGeneratedIDAsync<TEntity>(this DbContext context, string prefix, bool withSlash = true) where TEntity : class {
            // Use the first 3 letters of the class name as default prefix if none is provided
            if (string.IsNullOrEmpty(prefix)) {
                prefix = typeof(TEntity).Name.Substring(2, 3).ToUpper();
            }

            // Get the current count and increment by 1
            var count = await context.Set<TEntity>().CountAsync() + 1;

            // Extract the numeric portion from the prefix if it’s meant to be a number
            if (long.TryParse(prefix, out long baseNumber)) {

                // Increment the base number by the count
                var newIDNumber = baseNumber + count;

                // Return the new ID with or without the slash as specified
                return withSlash ? $"{newIDNumber.ToString().Insert(4, "-")}" : $"{newIDNumber}";
            }

            // Return the new ID with the format "<prefix>-00<count>"
            return withSlash ? $"{prefix}-00{count}" : $"{prefix}{count}";
        }
        public static async Task<string> GetGeneratedIDAsync<TEntity>(this DbContext context, string prefix, bool withSlash, Expression<Func<TEntity, bool>> whereCondition) where TEntity : class {
            // Use the first 3 letters of the class name as default prefix if none is provided
            if (string.IsNullOrEmpty(prefix)) {
                prefix = typeof(TEntity).Name.Substring(2, 3).ToUpper();
            }

            // Apply the where condition if provided
            var query = context.Set<TEntity>().AsQueryable();

            if (whereCondition != null) {
                query = query.Where(whereCondition);
            }

            // Get the count with the where condition, then increment by 1
            var count = await query.CountAsync() + 1;

            // Extract the numeric portion from the prefix if it’s meant to be a number
            if (long.TryParse(prefix, out long baseNumber)) {

                // Increment the base number by the count
                var newIDNumber = baseNumber + count;

                // Return the new ID with or without the slash as specified
                return withSlash ? $"{newIDNumber.ToString().Insert(4, "-")}" : $"{newIDNumber}";
            }

            // Return the new ID with the format "<prefix>-00<count>"
            return withSlash ? $"{prefix}-00{count}" : $"{prefix}{count}";
        }
        #endregion

        #region Get All Method
        public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this DbContext context, string propertyName, object propertyValue) where TEntity : class {

            Expression<Func<TEntity, bool>> whereCondition = null;

            if (!string.IsNullOrEmpty(propertyName)) {
                // Get the property info for the specified property name
                var property = typeof(TEntity).GetProperty(propertyName);

                if (property != null) {
                    // Create a parameter expression representing the entity (e.g., e => e.PropertyName)
                    var parameter = Expression.Parameter(typeof(TEntity), "e");

                    // Create an expression for accessing the property (e.PropertyName)
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);

                    // Create a constant expression for the value to compare against (propertyValue)
                    var constant = Expression.Constant(propertyValue);

                    // Create an equality expression (e.PropertyName == propertyValue)
                    var equality = Expression.Equal(propertyAccess, constant);

                    // Create a lambda expression representing the predicate (e => e.PropertyName == propertyValue)
                    whereCondition = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

                }
            }

            // If no property filter is provided or the property does not exist, return all entities
            return await context.GetAllAsync(whereCondition);
        }
        public static async Task<IEnumerable<TEntity>> GetAllWithSearchAsync<TEntity>(this DbContext context, string searchText, DateTime? dateFrom, DateTime? dateTo, int dataLimit = 100, bool isCached = true) where TEntity : class {

            var cachedData = await CacheExtensions.GetAllEnumerableCacheAsync<TEntity>();

            if (cachedData != null && isCached) {

                var result = cachedData.SearchDateRange(dateFrom, dateTo);

                result = result.SearchText(searchText);

                if (result != null) {

                    result = result.Take(dataLimit);

                    return result.ToList();

                }
            }

            var query = context.Set<TEntity>().AsQueryable();

            query = query.SearchDateRange(dateFrom, dateTo);

            query = query.SearchText(searchText);

            query = query.Take(dataLimit);

            return await query.ToListAsync();
        }
        public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this DbContext context, bool isCached = true) where TEntity : class {

            var cachedData = await CacheExtensions.GetAllQueryableCacheAsync<TEntity>();

            if (cachedData != null && isCached) {

                return cachedData;

            }

            return await context.Set<TEntity>().ToListAsync();

        }

        public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> whereCondition, bool isCached = true) where TEntity : class {

            var cachedData = await CacheExtensions.GetAllQueryableCacheAsync<TEntity>();

            if (cachedData != null && isCached) {

                var result = cachedData.Where(whereCondition);

                if (result != null && result.Any()) {

                    return result.ToList();

                }

            }

            // Get the DbSet for TEntity
            var dbSet = context.Set<TEntity>();

            // Apply the where condition if provided
            var query = dbSet.AsQueryable();

            query = query.Where(whereCondition);

            // If no Status property exists, return all entities
            return await query.ToListAsync();
        }
        #endregion

        #region Utilities
        public static async Task<bool> HasDataAsync<TEntity>(this DbContext context, Expression<Func<TEntity, bool>> predicate = null) where TEntity : class {
            return predicate != null ? await context.Set<TEntity>().AnyAsync(predicate) : await context.Set<TEntity>().AnyAsync();
        }
        #endregion

        #region Alter Table Columns
        public static async Task CreateOrUpdateTableAsync<TEntity>(this DbContext dbContext, params Expression<Func<TEntity, object>>[] excludeProperties) {
            // Get table name and properties
            var tableName = typeof(TEntity).Name;
            var properties = typeof(TEntity).GetProperties();

            // Exclude specified properties
            if (excludeProperties != null && excludeProperties.Length > 0) {
                var excludedNames = excludeProperties
                    .Select(GetPropertyName)
                    .ToHashSet();

                properties = properties
                    .Where(p => !excludedNames.Contains(p.Name))
                    .ToArray();
            }

            // Check if table exists
            if (!IsTableExists(dbContext, tableName)) {

                // 2. Build CREATE TABLE SQL dynamically
                var columnsSql = new List<string>();

                // Add columns
                foreach (var prop in properties) {
                    var columnName = prop.Name;
                    var columnType = GetMySqlColumnType(prop.PropertyType);
                    var isNullable = !IsNonNullable(prop.PropertyType);
                    var defaultValue = GetDefaultValue(prop, typeof(TEntity));

                    string columnDef = $"`{columnName}` {columnType} {(isNullable ? "NULL" : "NOT NULL")}";
                    if (defaultValue != null) {
                        columnDef += $" DEFAULT '{defaultValue}'";
                    }
                    columnsSql.Add(columnDef);
                }

                // Assume first property is primary key
                var primaryKey = properties.FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
                if (primaryKey != null) {
                    columnsSql.Add($"PRIMARY KEY (`{primaryKey.Name}`)");
                }
                // Final CREATE TABLE SQL
                var createTableSql = $"CREATE TABLE `{tableName}` ({string.Join(", ", columnsSql)});";
                // Execute the CREATE TABLE command
                await dbContext.Database.ExecuteSqlRawAsync(createTableSql);
            }
            else {

                // Table exists, alter columns as needed
                foreach (var prop in properties) {
                    // Determine column details
                    var columnName = prop.Name;
                    var columnType = GetMySqlColumnType(prop.PropertyType);
                    var isNullable = !IsNonNullable(prop.PropertyType);
                    var defaultValue = GetDefaultValue(prop, typeof(TEntity));

                    // Apply changes to the database
                    try {
                        // Check if column exists
                        if (IsColumnExists(dbContext, tableName, columnName)) {
                            // Alter existing column
                            await dbContext.Database.ExecuteSqlAsync(
                                $"ALTER TABLE `{tableName}` MODIFY COLUMN `{columnName}` {columnType} {(isNullable ? "NULL" : "NOT NULL")} {(defaultValue != null ? $"DEFAULT '{defaultValue}'" : "")};"
                            );
                        }
                        else {
                            // Add new column
                            await dbContext.Database.ExecuteSqlAsync(
                                $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnType} {(isNullable ? "NULL" : "NOT NULL")} {(defaultValue != null ? $"DEFAULT '{defaultValue}'" : "")};"
                            );
                        }
                    }
                    catch (Exception ex) {
                        continue;
                    }
                }
            }
        }

        private static string GetMySqlColumnType(Type type) {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(int)) return "INT";
            if (type == typeof(long)) return "BIGINT";
            if (type == typeof(short)) return "SMALLINT";
            if (type == typeof(byte)) return "TINYINT UNSIGNED";
            if (type == typeof(bool)) return "TINYINT(1)";
            if (type == typeof(decimal)) return "DECIMAL(18,2)";
            if (type == typeof(float)) return "FLOAT";
            if (type == typeof(double)) return "DOUBLE";
            if (type == typeof(string)) return "VARCHAR(255)";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(Guid)) return "CHAR(36)";
            if (type == typeof(byte[])) return "BLOB";
            // add more types if needed
            throw new NotSupportedException($"Type {type.Name} not supported");
        }

        private static bool IsNonNullable(Type type) {
            // If it's Nullable<T>, it's nullable
            if (Nullable.GetUnderlyingType(type) != null)
                return false;

            // Reference types are nullable
            if (!type.IsValueType)
                return false;

            // Value types (int, DateTime, bool, etc.) are non-nullable
            return true;
        }

        private static object GetDefaultValue(PropertyInfo prop, Type modelType) {

            var instance = Activator.CreateInstance(modelType);

            var value = prop.GetValue(instance);

            // Get CLR default for the property type
            var defaultValue = prop.PropertyType.IsValueType
                ? Activator.CreateInstance(prop.PropertyType)
                : null;

            // If the value equals the CLR default, treat it as "no default"
            if (Equals(value, defaultValue))
                return null;

            return value;
        }

        private static bool IsColumnExists(DbContext dbContext, string tableName, string columnName) {
            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_SCHEMA = DATABASE() 
                  AND TABLE_NAME = @tableName 
                  AND COLUMN_NAME = @columnName;
            ";

            var param1 = cmd.CreateParameter();
            param1.ParameterName = "@tableName";
            param1.Value = tableName;
            cmd.Parameters.Add(param1);

            var param2 = cmd.CreateParameter();
            param2.ParameterName = "@columnName";
            param2.Value = columnName;
            cmd.Parameters.Add(param2);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        private static bool IsTableExists(DbContext dbContext, string tableName) {
            var conn = dbContext.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = DATABASE() 
                  AND TABLE_NAME = @tableName;
    ";
            var param = cmd.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            cmd.Parameters.Add(param);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        private static string GetPropertyName<TEntity>(
            Expression<Func<TEntity, object>> expression) {
            if (expression.Body is MemberExpression member)
                return member.Member.Name;

            if (expression.Body is UnaryExpression unary &&
                unary.Operand is MemberExpression unaryMember)
                return unaryMember.Member.Name;

            throw new ArgumentException("Invalid property expression");
        }
        #endregion
    }
}
